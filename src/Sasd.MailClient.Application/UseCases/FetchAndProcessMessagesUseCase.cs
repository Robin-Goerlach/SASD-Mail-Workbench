using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Application.UseCases;

/// <summary>
/// Orchestriert den kompletten Erstablauf:
/// Quelle pruefen, neue Nachrichten finden, Rohdaten speichern, parsen und verarbeiten.
/// Diese Klasse ist bewusst ausfuehrlich kommentiert, weil sie spaeter ein zentraler
/// Anwendungsfall der GUI wird.
/// </summary>
public sealed class FetchAndProcessMessagesUseCase
{
    private readonly IMailSource _mailSource;
    private readonly IRawMessageStore _rawMessageStore;
    private readonly IMessageCatalogRepository _catalogRepository;
    private readonly IMessageParser _messageParser;
    private readonly IReadOnlyList<IMessageProcessor> _processors;
    private readonly IMessageFingerprintService _fingerprintService;
    private readonly IClock _clock;
    private readonly IApplicationLogger _logger;

    /// <summary>
    /// Baut den Use Case mit allen benoetigten Bausteinen auf.
    /// Die Use-Case-Klasse kennt nur Abstraktionen. Dadurch kann spaeter dieselbe
    /// Orchestrierung sowohl mit Demo-Dateiquelle als auch mit echtem POP3-Adapter laufen.
    /// </summary>
    public FetchAndProcessMessagesUseCase(
        IMailSource mailSource,
        IRawMessageStore rawMessageStore,
        IMessageCatalogRepository catalogRepository,
        IMessageParser messageParser,
        IReadOnlyList<IMessageProcessor> processors,
        IMessageFingerprintService fingerprintService,
        IClock clock,
        IApplicationLogger logger)
    {
        _mailSource = mailSource;
        _rawMessageStore = rawMessageStore;
        _catalogRepository = catalogRepository;
        _messageParser = messageParser;
        _processors = processors;
        _fingerprintService = fingerprintService;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Fuehrt einen kompletten Abruf- und Verarbeitungslauf aus.
    /// Fehler einzelner Nachrichten werden gesammelt, damit der Gesamtlauf moeglichst robust bleibt.
    /// </summary>
    public async Task<FetchRunReport> ExecuteAsync(
        MailAccount account,
        CancellationToken cancellationToken)
    {
        FetchRunReport report = new FetchRunReport();

        _logger.LogInformation($"Starte Abruflauf fuer Konto '{account.DisplayName}'.");

        MailSourceConnectionTestResult connectionResult =
            await _mailSource.TestConnectionAsync(account, cancellationToken);

        if (!connectionResult.Success)
        {
            string message = $"Quellverbindung fehlgeschlagen: {connectionResult.TechnicalMessage}";
            report.Errors.Add(message);
            _logger.LogError(message);
            return report;
        }

        IReadOnlyList<RemoteMessageInfo> remoteMessages =
            await _mailSource.ListAvailableMessagesAsync(account, cancellationToken);

        report.DiscoveredRemoteMessages = remoteMessages.Count;

        foreach (RemoteMessageInfo remoteMessage in remoteMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool alreadyKnown = await _catalogRepository.ExistsByExternalKeyAsync(
                account.Id,
                remoteMessage.ExternalKey,
                cancellationToken);

            if (alreadyKnown)
            {
                report.SkippedAlreadyKnownMessages++;
                continue;
            }

            try
            {
                // 1. Die Nachricht wird aus der Quelle geholt.
                //    Ab hier haben wir den kompletten Rohtext lokal verfuegbar.
                RemoteMessageContent content = await _mailSource.FetchMessageAsync(
                    account,
                    remoteMessage,
                    cancellationToken);

                // 2. Direkt auf dem Rohinhalt erzeugen wir einen Fingerprint.
                //    Das hilft spaeter bei Dubletten und bei Reprocessing-Szenarien.
                string fingerprint = _fingerprintService.CreateFingerprint(content.RawContent);

                RawMessage rawMessage = new RawMessage
                {
                    AccountId = account.Id,
                    ExternalMessageKey = remoteMessage.ExternalKey,
                    SourceIdentifier = remoteMessage.SourceIdentifier,
                    Fingerprint = fingerprint,
                    RetrievedAtUtc = _clock.UtcNow,
                    ApproximateSizeInBytes = remoteMessage.ApproximateSizeInBytes,
                    State = MessageProcessingState.Fetched
                };

                // 3. Rohinhalt getrennt speichern. So bleibt die Originalmail immer erhalten,
                //    selbst wenn spaeter Parser oder Verarbeiter Fehler machen.
                string storagePath = await _rawMessageStore.SaveAsync(
                    rawMessage,
                    content.RawContent,
                    cancellationToken);

                rawMessage.StoragePath = storagePath;
                rawMessage.State = MessageProcessingState.RawStored;

                await _catalogRepository.SaveRawMessageAsync(rawMessage, cancellationToken);

                // 4. Den Rohinhalt in eine normalisierte, fachlich verwendbare Nachricht parsen.
                ParsedMessage parsedMessage = await _messageParser.ParseAsync(
                    account,
                    rawMessage,
                    content.RawContent,
                    cancellationToken);

                parsedMessage.State = MessageProcessingState.Parsed;
                await _catalogRepository.SaveParsedMessageAsync(parsedMessage, cancellationToken);

                // 5. Die definierte Pipeline ueber die geparste Nachricht laufen lassen.
                foreach (IMessageProcessor processor in _processors)
                {
                    ProcessingResult processingResult = await processor.ProcessAsync(
                        parsedMessage,
                        cancellationToken);

                    await _catalogRepository.SaveProcessingResultAsync(
                        processingResult,
                        cancellationToken);

                    if (!processingResult.Success)
                    {
                        report.Warnings.Add(
                            $"Prozessor '{processor.Name}' meldete ein Problem fuer Nachricht {parsedMessage.Id}.");
                    }
                }

                // 6. Zum Schluss markieren wir die Nachricht als verarbeitet.
                parsedMessage.State = MessageProcessingState.Processed;
                await _catalogRepository.SaveParsedMessageAsync(parsedMessage, cancellationToken);

                report.NewlyImportedMessages++;
                report.ImportedMessageIds.Add(parsedMessage.Id);

                _logger.LogInformation(
                    $"Nachricht {parsedMessage.Id} erfolgreich importiert und verarbeitet.");
            }
            catch (Exception exception)
            {
                string message =
                    $"Nachricht mit externem Schluessel '{remoteMessage.ExternalKey}' konnte nicht sauber verarbeitet werden.";

                report.Errors.Add(message);
                _logger.LogError(message, exception);
            }
        }

        account.LastSuccessfulFetchUtc = _clock.UtcNow;

        _logger.LogInformation(
            $"Abruflauf abgeschlossen. Neu importiert: {report.NewlyImportedMessages}, uebersprungen: {report.SkippedAlreadyKnownMessages}.");

        return report;
    }
}
