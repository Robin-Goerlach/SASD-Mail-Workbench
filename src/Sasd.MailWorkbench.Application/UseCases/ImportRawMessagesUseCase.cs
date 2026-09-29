using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Application.Contracts.Import;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Application.UseCases;

/// <summary>
/// Orchestriert den inkrementellen Import und die vollständige bytegenaue Verifikation.
/// </summary>
/// <remarks>
/// Der Use Case kennt weder POP3 noch SQLite oder konkrete Dateipfade. Er arbeitet
/// ausschließlich mit Schnittstellen. Dadurch kann die lokale EML-Demoquelle in
/// Milestone 0.4.0 durch MailKit ersetzt werden, ohne die Importregeln neu zu schreiben.
/// </remarks>
public sealed class ImportRawMessagesUseCase
{
    private readonly IRawMessageSource _source;
    private readonly IRawMessageStore _store;
    private readonly IMessageImportRepository _repository;
    private readonly IClock _clock;
    private readonly IApplicationLogger _logger;

    /// <summary>
    /// Erstellt den Use Case mit seinen technischen Adaptern.
    /// </summary>
    public ImportRawMessagesUseCase(
        IRawMessageSource source,
        IRawMessageStore store,
        IMessageImportRepository repository,
        IClock clock,
        IApplicationLogger logger)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Importiert alle relevanten Quellenobjekte.
    /// </summary>
    /// <param name="request">Konto und gewünschte Importstrategie.</param>
    /// <param name="cancellationToken">Token für einen kontrollierten Abbruch.</param>
    /// <returns>Ein UI-unabhängiger Bericht über den gesamten Lauf.</returns>
    /// <remarks>
    /// Im inkrementellen Modus dient der Quellschlüssel als schnelle Optimierung.
    /// Im vollständigen Modus wird jedes sichtbare Objekt erneut gestreamt und über
    /// SHA-256 plus Byte-Länge geprüft. Genau dieser Modus bildet später „Get all“.
    /// </remarks>
    public async Task<ImportRunReport> ExecuteAsync(
        ImportRawMessagesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.AccountId))
        {
            throw new ArgumentException("Die Konto-ID darf nicht leer sein.", nameof(request));
        }

        ImportRunReport report = new() { StartedAtUtc = _clock.UtcNow };

        try
        {
            await _repository.InitializeAsync(cancellationToken).ConfigureAwait(false);

            IReadOnlyList<RawMessageSourceItem> items = await _source
                .ListAsync(request.AccountId, cancellationToken)
                .ConfigureAwait(false);
            report.Discovered = items.Count;

            foreach (RawMessageSourceItem item in items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    report.Complete(_clock.UtcNow, ImportRunOutcome.Cancelled);
                    return report;
                }

                if (request.Mode == ImportMode.Incremental)
                {
                    // UIDL beziehungsweise SourceKey ist nur ein schneller Vorfilter.
                    // Der vollständige Modus überspringt diese Abkürzung absichtlich.
                    StoredRawMessage? known = await _repository
                        .FindBySourceKeyAsync(request.AccountId, item.SourceKey, cancellationToken)
                        .ConfigureAwait(false);

                    if (known is not null)
                    {
                        report.SkippedKnownSourceKeys++;
                        continue;
                    }
                }

                await ImportSingleAsync(request.AccountId, item, report, cancellationToken)
                    .ConfigureAwait(false);

                if (report.Outcome == ImportRunOutcome.Cancelled)
                {
                    return report;
                }
            }

            ImportRunOutcome outcome = report.Failed > 0
                ? ImportRunOutcome.CompletedWithErrors
                : ImportRunOutcome.Completed;
            report.Complete(_clock.UtcNow, outcome);
            return report;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            report.Complete(_clock.UtcNow, ImportRunOutcome.Cancelled);
            return report;
        }
        catch (Exception exception)
        {
            // Ein Fehler beim Initialisieren oder Auflisten betrifft den gesamten Lauf
            // und kann keiner einzelnen Nachricht sicher zugeordnet werden.
            report.AddError(new ImportRunError("*", "IMPORT_RUN_FAILED", exception.Message));
            report.Complete(_clock.UtcNow, ImportRunOutcome.Failed);
            _logger.Error("IMPORT_RUN_FAILED", "Der Importlauf konnte nicht abgeschlossen werden.", exception);
            return report;
        }
    }

    private async Task ImportSingleAsync(
        string accountId,
        RawMessageSourceItem item,
        ImportRunReport report,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = _clock.UtcNow;
        ImportAttempt attempt = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            AccountId = accountId,
            SourceKey = item.SourceKey,
            State = ImportAttemptState.Discovered,
            StartedAtUtc = now,
            UpdatedAtUtc = now
        };
        bool attemptPersisted = false;

        try
        {
            await _repository.CreateAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);
            attemptPersisted = true;

            attempt = attempt with
            {
                State = ImportAttemptState.Downloading,
                UpdatedAtUtc = _clock.UtcNow
            };
            await _repository.UpdateAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);

            await using Stream sourceStream = await _source
                .OpenReadAsync(accountId, item, cancellationToken)
                .ConfigureAwait(false);

            // StageAsync schreibt die Originalbytes und berechnet den Fingerprint
            // in einem einzigen Durchlauf. Die Mail wird nicht als Text dekodiert.
            StagedRawMessage staged = await _store
                .StageAsync(sourceStream, cancellationToken)
                .ConfigureAwait(false);

            attempt = attempt with
            {
                State = ImportAttemptState.Staged,
                StagingRelativePath = staged.StagingRelativePath,
                Fingerprint = staged.Fingerprint,
                UpdatedAtUtc = _clock.UtcNow
            };
            await _repository.UpdateAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);

            StoredRawMessage? duplicate = await _repository
                .FindByFingerprintAsync(accountId, staged.Fingerprint, cancellationToken)
                .ConfigureAwait(false);

            if (duplicate is not null)
            {
                // Nur die neue temporäre lokale Kopie wird entfernt. Weder die
                // kanonische lokale Nachricht noch eine Servermail werden gelöscht.
                await _store
                    .DiscardAsync(staged.StagingRelativePath, cancellationToken)
                    .ConfigureAwait(false);

                SourceObservation observation = CreateObservation(attempt, duplicate.Id);
                await _repository
                    .CompleteDuplicateAsync(attempt, duplicate, observation, CancellationToken.None)
                    .ConfigureAwait(false);
                report.ExactDuplicates++;
                return;
            }

            string targetPath = _store.GetTargetRelativePath(attempt.Id, _clock.UtcNow);
            attempt = attempt with
            {
                State = ImportAttemptState.Committing,
                TargetRelativePath = targetPath,
                UpdatedAtUtc = _clock.UtcNow
            };
            await _repository.UpdateAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);

            // Staging und Ziel liegen auf demselben Dateisystem. File.Move ist
            // dort eine atomare Umbenennung: Entweder existiert die alte oder die
            // neue Datei, niemals eine nur teilweise kopierte Zielmail.
            await _store
                .CommitAsync(staged.StagingRelativePath, targetPath, cancellationToken)
                .ConfigureAwait(false);

            StoredRawMessage candidate = new(
                attempt.Id,
                accountId,
                staged.Fingerprint,
                targetPath,
                _clock.UtcNow);
            SourceObservation newObservation = CreateObservation(attempt, candidate.Id);

            // Der Katalogabschluss ist transaktional. Ein paralleler Import kann
            // denselben Fingerprint bereits gewonnen haben; dann wird unten nur
            // unsere nicht-kanonische Datei entfernt.
            CatalogCompletionResult completion = await _repository
                .CompleteNewMessageAsync(attempt, candidate, newObservation, CancellationToken.None)
                .ConfigureAwait(false);

            if (completion.CandidateWasCreated)
            {
                report.NewlyStored++;
            }
            else
            {
                await TryDiscardOrphanCandidateAsync(targetPath).ConfigureAwait(false);
                report.ExactDuplicates++;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (attemptPersisted)
            {
                await TryMarkAttemptAsync(
                    attempt,
                    ImportAttemptState.Cancelled,
                    "IMPORT_CANCELLED",
                    "Der Import wurde durch den Benutzer abgebrochen.")
                    .ConfigureAwait(false);
            }

            report.Complete(_clock.UtcNow, ImportRunOutcome.Cancelled);
        }
        catch (IOException exception)
        {
            // Während Downloading umfasst der Vorgang Lesen plus Schreiben in die
            // temporäre Datei. Erst ab Staged/Committing ist es eindeutig ein
            // Fehler der dauerhaften lokalen Speicherung.
            ImportAttemptState failureState = attempt.State == ImportAttemptState.Downloading
                ? ImportAttemptState.DownloadFailed
                : ImportAttemptState.StorageFailed;
            string code = failureState == ImportAttemptState.DownloadFailed
                ? "DOWNLOAD_IO_ERROR"
                : "STORAGE_IO_ERROR";

            if (attemptPersisted)
            {
                await TryMarkAttemptAsync(attempt, failureState, code, exception.Message)
                    .ConfigureAwait(false);
            }

            report.AddError(new ImportRunError(item.SourceKey, code, exception.Message));
            _logger.Error(code, $"E/A-Fehler bei '{item.DisplayName}'.", exception);
        }
        catch (Exception exception)
        {
            ImportAttemptState failureState = attempt.State == ImportAttemptState.Downloading
                ? ImportAttemptState.DownloadFailed
                : ImportAttemptState.RequiresManualReview;

            if (attemptPersisted)
            {
                await TryMarkAttemptAsync(
                    attempt,
                    failureState,
                    "IMPORT_UNEXPECTED",
                    exception.Message).ConfigureAwait(false);
            }

            report.AddError(new ImportRunError(item.SourceKey, "IMPORT_UNEXPECTED", exception.Message));
            _logger.Error("IMPORT_UNEXPECTED", $"Unerwarteter Fehler bei '{item.DisplayName}'.", exception);
        }
    }

    private async Task TryDiscardOrphanCandidateAsync(string relativePath)
    {
        try
        {
            await _store.DiscardAsync(relativePath, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            // Der Katalog ist bereits konsistent. Eine übrig gebliebene Datei ist
            // deshalb ein Wartungsbefund, aber kein Grund, die kanonische Mail zu verwerfen.
            _logger.Warning(
                "IMPORT_ORPHAN_CLEANUP_REQUIRED",
                $"Die nicht-kanonische Datei '{relativePath}' konnte nicht entfernt werden: {exception.Message}");
        }
    }

    private SourceObservation CreateObservation(ImportAttempt attempt, string messageId)
    {
        DateTimeOffset now = _clock.UtcNow;
        return new SourceObservation(
            attempt.AccountId,
            attempt.SourceKey,
            messageId,
            now,
            now);
    }

    private async Task TryMarkAttemptAsync(
        ImportAttempt attempt,
        ImportAttemptState state,
        string code,
        string message)
    {
        ImportAttempt updated = attempt with
        {
            State = state,
            UpdatedAtUtc = _clock.UtcNow,
            CompletedAtUtc = state is ImportAttemptState.Cancelled or ImportAttemptState.RequiresManualReview
                ? _clock.UtcNow
                : null,
            ErrorCode = code,
            ErrorMessage = message,
            RetryCount = attempt.RetryCount + 1
        };

        try
        {
            // Fehlerbehandlung darf den ursprünglichen Fehler nicht verdecken.
            // Deshalb wird ein Fehlschlag beim Speichern des Fehlerzustands nur
            // protokolliert und nicht erneut nach außen geworfen.
            await _repository.UpdateAttemptAsync(updated, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception persistenceException)
        {
            _logger.Error(
                "IMPORT_STATE_PERSIST_FAILED",
                $"Der Fehlerzustand des Importversuchs '{attempt.Id}' konnte nicht gespeichert werden.",
                persistenceException);
        }
    }
}
