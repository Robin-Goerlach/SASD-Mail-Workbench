using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Application.UseCases;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;
using Sasd.MailClient.Infrastructure.Mail;
using Sasd.MailClient.Infrastructure.Parsing;
using Sasd.MailClient.Infrastructure.Processing;
using Sasd.MailClient.Infrastructure.Services;
using Sasd.MailClient.Infrastructure.Storage;
using Sasd.MailClient.Tests.TestSupport;
using Sasd.MailClient.Tests.Testing;

namespace Sasd.MailClient.Tests.Suites;

/// <summary>
/// Prueft den End-to-End-Ablauf des zentralen Use Cases.
/// Diese Tests sind fuer den Projektstart besonders wertvoll, weil sie das Zusammenspiel
/// mehrerer Schichten absichern.
/// </summary>
public static class UseCaseTests
{
    public static IEnumerable<TestCaseDefinition> GetCases()
    {
        yield return new TestCaseDefinition
        {
            Name = "Use Case importiert, parst und verarbeitet Demo-Mails",
            ExecuteAsync = ImportAndProcessMessagesAsync
        };

        yield return new TestCaseDefinition
        {
            Name = "Use Case ueberspringt beim zweiten Lauf bereits bekannte Mails",
            ExecuteAsync = SecondRunSkipsKnownMessagesAsync
        };

        yield return new TestCaseDefinition
        {
            Name = "Fingerprint-Service erzeugt fuer gleichen Inhalt denselben Wert",
            ExecuteAsync = FingerprintServiceIsDeterministicAsync
        };
    }

    private static async Task ImportAndProcessMessagesAsync()
    {
        using TestWorkspace workspace = new TestWorkspace("use-case-import-process");
        await TestMailFactory.SeedInboxAsync(workspace.InboxDirectory);

        FetchAndProcessMessagesUseCase useCase = BuildUseCase(workspace, out IMessageCatalogRepository repository);
        MailAccount account = CreateAccount();

        FetchRunReport report = await useCase.ExecuteAsync(account, CancellationToken.None);
        IReadOnlyList<ParsedMessage> parsedMessages = await repository.ListParsedMessagesAsync(CancellationToken.None);

        AssertEx.Equal(2, report.DiscoveredRemoteMessages, "Der Use Case sollte zwei Remote-Mails gefunden haben.");
        AssertEx.Equal(2, report.NewlyImportedMessages, "Der Use Case sollte zwei neue Nachrichten importiert haben.");
        AssertEx.Equal(2, parsedMessages.Count, "Es sollten zwei geparste Nachrichten gespeichert worden sein.");

        ParsedMessage? invoiceMessage = parsedMessages.FirstOrDefault(message => message.Subject.Contains("Rechnung", StringComparison.OrdinalIgnoreCase));
        AssertEx.NotNull(invoiceMessage, "Die erwartete Rechnungsnachricht wurde nicht gefunden.");
        AssertEx.True(invoiceMessage!.Tags.Any(tag => tag == "finance"), "Die Rechnungsnachricht sollte das Tag 'finance' erhalten haben.");

        IReadOnlyList<ProcessingResult> results = await repository.ListProcessingResultsAsync(invoiceMessage.Id, CancellationToken.None);
        ProcessingResult? extractionResult = results.FirstOrDefault(result => result.ProcessorName == nameof(KeyValueBodyExtractionProcessor));

        AssertEx.NotNull(extractionResult, "Das Extraktionsergebnis des KeyValueBodyExtractionProcessor fehlt.");
        AssertEx.Equal("INV-2026-0007", extractionResult!.ExtractedData["Invoice"], "Die Invoice-Nummer wurde nicht korrekt extrahiert.");
    }

    private static async Task SecondRunSkipsKnownMessagesAsync()
    {
        using TestWorkspace workspace = new TestWorkspace("use-case-second-run-skips");
        await TestMailFactory.SeedInboxAsync(workspace.InboxDirectory);

        FetchAndProcessMessagesUseCase useCase = BuildUseCase(workspace, out IMessageCatalogRepository repository);
        MailAccount account = CreateAccount();

        FetchRunReport firstRun = await useCase.ExecuteAsync(account, CancellationToken.None);
        FetchRunReport secondRun = await useCase.ExecuteAsync(account, CancellationToken.None);

        AssertEx.Equal(2, firstRun.NewlyImportedMessages, "Im ersten Lauf haetten zwei Mails importiert werden muessen.");
        AssertEx.Equal(0, secondRun.NewlyImportedMessages, "Im zweiten Lauf duerfen keine neuen Mails mehr importiert werden.");
        AssertEx.Equal(2, secondRun.SkippedAlreadyKnownMessages, "Im zweiten Lauf haetten beide bereits bekannten Mails uebersprungen werden muessen.");

        IReadOnlyList<ParsedMessage> parsedMessages = await repository.ListParsedMessagesAsync(CancellationToken.None);
        AssertEx.Equal(2, parsedMessages.Count, "Es duerfen nach zwei Laeufen nicht ploetzlich mehr als zwei Nachrichten im Katalog liegen.");
    }

    private static Task FingerprintServiceIsDeterministicAsync()
    {
        Sha256MessageFingerprintService service = new Sha256MessageFingerprintService();

        string content = TestMailFactory.CreatePlainTextProjectMail();
        string first = service.CreateFingerprint(content);
        string second = service.CreateFingerprint(content);
        string third = service.CreateFingerprint(content + "\n#change");

        AssertEx.Equal(first, second, "Der gleiche Rohinhalt muss denselben Fingerprint liefern.");
        AssertEx.False(string.Equals(first, third, StringComparison.Ordinal), "Veraenderter Inhalt sollte nicht denselben Fingerprint behalten.");
        return Task.CompletedTask;
    }

    private static FetchAndProcessMessagesUseCase BuildUseCase(TestWorkspace workspace, out IMessageCatalogRepository repository)
    {
        IMailSource mailSource = new DirectoryMailSource(workspace.InboxDirectory);
        IRawMessageStore rawMessageStore = new FileSystemRawMessageStore(workspace.RawMailDirectory);
        repository = new SqliteMessageCatalogRepository(workspace.DatabaseFilePath);
        IMessageParser parser = new SimpleMimeMessageParser();

        IReadOnlyList<IMessageProcessor> processors = new List<IMessageProcessor>
        {
            new NormalizeMessageProcessor(),
            new BasicTaggingProcessor(),
            new KeyValueBodyExtractionProcessor()
        };

        IMessageFingerprintService fingerprintService = new Sha256MessageFingerprintService();
        IClock clock = new FixedClock(new DateTimeOffset(2026, 3, 21, 12, 0, 0, TimeSpan.Zero));
        IApplicationLogger logger = new InMemoryApplicationLogger();

        return new FetchAndProcessMessagesUseCase(
            mailSource,
            rawMessageStore,
            repository,
            parser,
            processors,
            fingerprintService,
            clock,
            logger);
    }

    private static MailAccount CreateAccount()
    {
        return new MailAccount
        {
            Id = "account-demo",
            DisplayName = "Testkonto",
            Host = "samples.local",
            Port = 0,
            SecurityMode = MailSecurityMode.None,
            UserName = "demo-user"
        };
    }
}
