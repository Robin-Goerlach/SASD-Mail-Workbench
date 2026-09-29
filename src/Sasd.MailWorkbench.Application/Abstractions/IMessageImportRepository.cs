using Sasd.MailWorkbench.Domain.Models;
using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Persistiert kanonische Rohnachrichten, Quellenbeobachtungen und Importversuche.
/// </summary>
public interface IMessageImportRepository
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task<StoredRawMessage?> FindBySourceKeyAsync(
        string accountId,
        string sourceKey,
        CancellationToken cancellationToken);

    Task<StoredRawMessage?> FindByFingerprintAsync(
        string accountId,
        MessageFingerprint fingerprint,
        CancellationToken cancellationToken);

    Task CreateAttemptAsync(
        ImportAttempt attempt,
        CancellationToken cancellationToken);

    Task UpdateAttemptAsync(
        ImportAttempt attempt,
        CancellationToken cancellationToken);

    Task<CatalogCompletionResult> CompleteNewMessageAsync(
        ImportAttempt attempt,
        StoredRawMessage candidateMessage,
        SourceObservation observation,
        CancellationToken cancellationToken);

    Task CompleteDuplicateAsync(
        ImportAttempt attempt,
        StoredRawMessage existingMessage,
        SourceObservation observation,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ImportAttempt>> ListRecoverableAttemptsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StoredRawMessage>> ListMessagesAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Ergebnis eines transaktionalen Katalogabschlusses. Bei einem parallelen
/// Konflikt kann bereits eine andere kanonische Nachricht gewonnen haben.
/// </summary>
public sealed record CatalogCompletionResult(
    bool CandidateWasCreated,
    StoredRawMessage CanonicalMessage);
