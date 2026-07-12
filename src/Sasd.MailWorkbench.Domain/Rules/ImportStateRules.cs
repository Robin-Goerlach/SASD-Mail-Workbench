using Sasd.MailWorkbench.Domain.Enums;

namespace Sasd.MailWorkbench.Domain.Rules;

/// <summary>
/// Zentralisiert die erlaubten Übergänge der Import-Zustandsmaschine.
/// </summary>
/// <remarks>
/// Die Regeln liegen absichtlich in der Domain-Schicht. Dadurch kann weder die
/// SQLite-Implementierung noch eine spätere GUI einen eigentlich unzulässigen
/// Zustandswechsel stillschweigend durchführen.
/// </remarks>
public static class ImportStateRules
{
    private static readonly IReadOnlyDictionary<ImportAttemptState, ImportAttemptState[]> AllowedTransitions =
        new Dictionary<ImportAttemptState, ImportAttemptState[]>
        {
            [ImportAttemptState.Discovered] =
                [ImportAttemptState.Downloading, ImportAttemptState.Cancelled],
            [ImportAttemptState.Downloading] =
                [ImportAttemptState.Staged, ImportAttemptState.DownloadFailed, ImportAttemptState.Cancelled],
            [ImportAttemptState.Staged] =
                [ImportAttemptState.Committing, ImportAttemptState.DuplicateDiscarded,
                 ImportAttemptState.StorageFailed, ImportAttemptState.Cancelled,
                 ImportAttemptState.RequiresManualReview],
            [ImportAttemptState.Committing] =
                [ImportAttemptState.Completed, ImportAttemptState.DuplicateDiscarded,
                 ImportAttemptState.StorageFailed, ImportAttemptState.Cancelled,
                 ImportAttemptState.RequiresManualReview],
            [ImportAttemptState.DownloadFailed] =
                [ImportAttemptState.Downloading, ImportAttemptState.RequiresManualReview],
            [ImportAttemptState.StorageFailed] =
                [ImportAttemptState.Committing, ImportAttemptState.RequiresManualReview],
            [ImportAttemptState.Cancelled] =
                [ImportAttemptState.Downloading],
            [ImportAttemptState.RequiresManualReview] =
                [ImportAttemptState.Downloading, ImportAttemptState.Committing],
            [ImportAttemptState.Completed] = [],
            [ImportAttemptState.DuplicateDiscarded] = []
        };

    /// <summary>
    /// Prüft, ob ein gewünschter Zustandswechsel fachlich zulässig ist.
    /// </summary>
    /// <param name="from">Aktuell gespeicherter Zustand.</param>
    /// <param name="to">Gewünschter Folgezustand.</param>
    /// <returns><see langword="true"/>, wenn der Übergang erlaubt ist.</returns>
    public static bool CanTransition(ImportAttemptState from, ImportAttemptState to) =>
        AllowedTransitions.TryGetValue(from, out ImportAttemptState[]? states) && states.Contains(to);

    /// <summary>
    /// Prüft, ob der Import fachlich endgültig abgeschlossen ist.
    /// </summary>
    public static bool IsTerminal(ImportAttemptState state) =>
        state is ImportAttemptState.Completed or ImportAttemptState.DuplicateDiscarded;

    /// <summary>
    /// Prüft, ob ein Start-Recovery den Zustand automatisch untersuchen muss.
    /// </summary>
    public static bool RequiresRecovery(ImportAttemptState state) =>
        state is ImportAttemptState.Downloading or ImportAttemptState.Staged or ImportAttemptState.Committing;
}
