namespace Sasd.MailWorkbench.Application.Contracts.Recovery;

/// <summary>
/// Beschreibt das Ergebnis des automatischen Wiederanlaufs unvollständiger Importe.
/// </summary>
public sealed class RecoveryReport
{
    /// <summary>
    /// Anzahl untersuchter Importversuche.
    /// </summary>
    public int Examined { get; internal set; }

    /// <summary>
    /// Anzahl vollständig und automatisch wiederhergestellter Importe.
    /// </summary>
    public int Recovered { get; internal set; }

    /// <summary>
    /// Anzahl unterbrochener Downloads, die für einen späteren Neuabruf markiert wurden.
    /// </summary>
    public int MarkedFailed { get; internal set; }

    /// <summary>
    /// Anzahl der Fälle, die nicht sicher automatisch entschieden werden konnten.
    /// </summary>
    public int RequiresManualReview { get; internal set; }

    /// <summary>
    /// Anzahl verwaister temporärer Dateien, die sicher entfernt wurden.
    /// </summary>
    public int OrphanStagingFilesDiscarded { get; internal set; }

    /// <summary>
    /// Zusätzliche verständliche Hinweise zum Recovery-Lauf.
    /// </summary>
    public List<string> Messages { get; } = [];
}
