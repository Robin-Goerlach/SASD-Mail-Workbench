namespace Sasd.MailWorkbench.Domain.Enums;

/// <summary>
/// Beschreibt das Gesamtergebnis eines Importlaufs.
/// </summary>
public enum ImportRunOutcome
{
    /// <summary>Alle relevanten Nachrichten wurden ohne Objektfehler bearbeitet.</summary>
    Completed = 0,

    /// <summary>Der Lauf wurde beendet, mindestens ein Objekt ist jedoch fehlgeschlagen.</summary>
    CompletedWithErrors = 1,

    /// <summary>Der Lauf wurde kontrolliert abgebrochen.</summary>
    Cancelled = 2,

    /// <summary>Der Lauf konnte als Ganzes nicht sinnvoll begonnen oder abgeschlossen werden.</summary>
    Failed = 3
}
