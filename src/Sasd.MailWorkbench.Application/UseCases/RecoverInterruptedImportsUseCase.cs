using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Application.Contracts.Recovery;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Application.UseCases;

/// <summary>
/// Untersucht beim Programmstart unvollständige Importversuche und führt nur
/// solche Schritte automatisch fort, deren Ergebnis eindeutig bestimmbar ist.
/// </summary>
public sealed class RecoverInterruptedImportsUseCase
{
    private readonly IRawMessageStore _store;
    private readonly IMessageImportRepository _repository;
    private readonly IClock _clock;
    private readonly IApplicationLogger _logger;

    /// <summary>
    /// Erstellt den Recovery-Use-Case.
    /// </summary>
    public RecoverInterruptedImportsUseCase(
        IRawMessageStore store,
        IMessageImportRepository repository,
        IClock clock,
        IApplicationLogger logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Führt die Wiederanlaufprüfung aus.
    /// </summary>
    /// <remarks>
    /// Das Verfahren ist konservativ: Bei fehlenden Metadaten oder widersprüchlichen
    /// Dateien wird nicht geraten. Der Versuch wird stattdessen als manuell zu prüfen
    /// markiert, damit keine Originalmail versehentlich gelöscht wird.
    /// </remarks>
    public async Task<RecoveryReport> ExecuteAsync(CancellationToken cancellationToken)
    {
        await _repository.InitializeAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<ImportAttempt> attempts = await _repository
            .ListRecoverableAttemptsAsync(cancellationToken)
            .ConfigureAwait(false);

        // Die Referenzmenge wird vor der Recovery gebildet. So dürfen wir später nur
        // Dateien löschen, die zu keinem bekannten offenen Importversuch gehören.
        HashSet<string> referencedStagingFiles = attempts
            .Select(attempt => attempt.StagingRelativePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        RecoveryReport report = new() { Examined = attempts.Count };

        foreach (ImportAttempt attempt in attempts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (attempt.State)
            {
                case ImportAttemptState.Downloading:
                    // Eine teilweise geschriebene Nachricht besitzt noch keinen
                    // verlässlichen Fingerprint. Sie wird beim nächsten Abruf neu geladen.
                    await MarkFailedAsync(attempt, "RECOVERY_INTERRUPTED_DOWNLOAD", cancellationToken)
                        .ConfigureAwait(false);
                    report.MarkedFailed++;
                    break;

                case ImportAttemptState.Staged:
                case ImportAttemptState.Committing:
                    await RecoverStagedOrCommittingAsync(attempt, report, cancellationToken)
                        .ConfigureAwait(false);
                    break;
            }
        }

        await DiscardUnreferencedStagingFilesAsync(
            referencedStagingFiles,
            report,
            cancellationToken).ConfigureAwait(false);

        return report;
    }

    private async Task RecoverStagedOrCommittingAsync(
        ImportAttempt attempt,
        RecoveryReport report,
        CancellationToken cancellationToken)
    {
        if (attempt.Fingerprint is null || string.IsNullOrWhiteSpace(attempt.StagingRelativePath))
        {
            await MarkManualReviewAsync(attempt, "RECOVERY_METADATA_INCOMPLETE", cancellationToken)
                .ConfigureAwait(false);
            report.RequiresManualReview++;
            return;
        }

        StoredRawMessage? duplicate = await _repository
            .FindByFingerprintAsync(attempt.AccountId, attempt.Fingerprint, cancellationToken)
            .ConfigureAwait(false);

        if (duplicate is not null)
        {
            // Der Katalog besitzt bereits eine kanonische Fassung. Lokale Reste dieses
            // Versuchs dürfen daher entfernt und als Duplikat abgeschlossen werden.
            await DiscardIfPresentAsync(attempt.StagingRelativePath, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(attempt.TargetRelativePath))
            {
                await DiscardIfPresentAsync(attempt.TargetRelativePath, cancellationToken)
                    .ConfigureAwait(false);
            }

            SourceObservation observation = NewObservation(attempt, duplicate.Id);
            await _repository
                .CompleteDuplicateAsync(attempt, duplicate, observation, CancellationToken.None)
                .ConfigureAwait(false);
            report.Recovered++;
            return;
        }

        string target = attempt.TargetRelativePath
            ?? _store.GetTargetRelativePath(attempt.Id, attempt.StartedAtUtc);
        bool targetExists = await _store.ExistsAsync(target, cancellationToken).ConfigureAwait(false);
        bool stageExists = await _store
            .ExistsAsync(attempt.StagingRelativePath, cancellationToken)
            .ConfigureAwait(false);

        if (!targetExists && stageExists)
        {
            // Der sichere Normalfall: Die vollständige Stagingdatei existiert noch.
            // Wir setzen zuerst den Katalogzustand und verschieben danach atomar.
            ImportAttempt committing = attempt with
            {
                State = ImportAttemptState.Committing,
                TargetRelativePath = target,
                UpdatedAtUtc = _clock.UtcNow
            };
            await _repository.UpdateAttemptAsync(committing, cancellationToken).ConfigureAwait(false);
            await _store.CommitAsync(attempt.StagingRelativePath, target, cancellationToken)
                .ConfigureAwait(false);
            attempt = committing;
            targetExists = true;
        }

        if (!targetExists)
        {
            // Weder Staging- noch Zieldatei ist vorhanden. Ohne Originalbytes darf
            // kein Katalogeintrag erfunden werden.
            await MarkManualReviewAsync(attempt, "RECOVERY_FILES_MISSING", cancellationToken)
                .ConfigureAwait(false);
            report.RequiresManualReview++;
            return;
        }

        if (attempt.State != ImportAttemptState.Committing)
        {
            attempt = attempt with
            {
                State = ImportAttemptState.Committing,
                TargetRelativePath = target,
                UpdatedAtUtc = _clock.UtcNow
            };
            await _repository.UpdateAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);
        }

        StoredRawMessage candidate = new(
            attempt.Id,
            attempt.AccountId,
            attempt.Fingerprint,
            target,
            _clock.UtcNow);
        CatalogCompletionResult completion = await _repository
            .CompleteNewMessageAsync(
                attempt,
                candidate,
                NewObservation(attempt, candidate.Id),
                CancellationToken.None)
            .ConfigureAwait(false);

        if (!completion.CandidateWasCreated)
        {
            await TryDiscardOrphanCandidateAsync(target).ConfigureAwait(false);
        }

        report.Recovered++;
    }

    private async Task DiscardUnreferencedStagingFilesAsync(
        IReadOnlySet<string> referencedStagingFiles,
        RecoveryReport report,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> files = await _store
            .ListStagingFilesAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (string file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (referencedStagingFiles.Contains(file))
            {
                continue;
            }

            await _store.DiscardAsync(file, cancellationToken).ConfigureAwait(false);
            report.OrphanStagingFilesDiscarded++;
            report.Messages.Add($"Verwaiste Stagingdatei entfernt: {file}");
        }
    }

    private async Task DiscardIfPresentAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        if (await _store.ExistsAsync(relativePath, cancellationToken).ConfigureAwait(false))
        {
            await _store.DiscardAsync(relativePath, cancellationToken).ConfigureAwait(false);
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
            _logger.Warning(
                "RECOVERY_ORPHAN_CLEANUP_REQUIRED",
                $"Die nicht-kanonische Datei '{relativePath}' konnte nicht entfernt werden: {exception.Message}");
        }
    }

    private SourceObservation NewObservation(ImportAttempt attempt, string messageId)
    {
        DateTimeOffset now = _clock.UtcNow;
        return new SourceObservation(attempt.AccountId, attempt.SourceKey, messageId, now, now);
    }

    private async Task MarkFailedAsync(
        ImportAttempt attempt,
        string code,
        CancellationToken cancellationToken)
    {
        await _repository.UpdateAttemptAsync(attempt with
        {
            State = ImportAttemptState.DownloadFailed,
            UpdatedAtUtc = _clock.UtcNow,
            RetryCount = attempt.RetryCount + 1,
            ErrorCode = code,
            ErrorMessage = "Ein unterbrochener Download wird bei einem späteren Abruf erneut gestartet."
        }, cancellationToken).ConfigureAwait(false);

        _logger.Warning("RECOVERY_DOWNLOAD_RESET", $"Importversuch {attempt.Id} wurde zurückgesetzt.");
    }

    private async Task MarkManualReviewAsync(
        ImportAttempt attempt,
        string code,
        CancellationToken cancellationToken)
    {
        await _repository.UpdateAttemptAsync(attempt with
        {
            State = ImportAttemptState.RequiresManualReview,
            UpdatedAtUtc = _clock.UtcNow,
            CompletedAtUtc = _clock.UtcNow,
            ErrorCode = code,
            ErrorMessage = "Der Importzustand konnte nicht sicher automatisch rekonstruiert werden."
        }, cancellationToken).ConfigureAwait(false);
    }
}
