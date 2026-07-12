namespace Sasd.MailWorkbench.Domain.Enums;

/// <summary>
/// Beschreibt den wiederanlaufbaren Lebenszyklus eines einzelnen Rohmail-Imports.
/// </summary>
public enum ImportAttemptState
{
    /// <summary>Die Quelle wurde gefunden, aber noch nicht geöffnet.</summary>
    Discovered = 0,

    /// <summary>Die Nachricht wird gelesen und in eine temporäre Datei übertragen.</summary>
    Downloading = 1,

    /// <summary>Die temporäre Datei wurde vollständig geschrieben und gehasht.</summary>
    Staged = 2,

    /// <summary>Die Stagingdatei wird in den kanonischen Speicher übernommen.</summary>
    Committing = 3,

    /// <summary>Die neue Nachricht wurde vollständig in Datei und Katalog übernommen.</summary>
    Completed = 4,

    /// <summary>Die Nachricht war bytegenau bekannt; nur die neue Kopie wurde verworfen.</summary>
    DuplicateDiscarded = 5,

    /// <summary>Lesen oder Übertragen der Nachricht ist fehlgeschlagen.</summary>
    DownloadFailed = 6,

    /// <summary>Die dauerhafte lokale Speicherung ist fehlgeschlagen.</summary>
    StorageFailed = 7,

    /// <summary>Der Benutzer oder die Anwendung hat den Vorgang kontrolliert abgebrochen.</summary>
    Cancelled = 8,

    /// <summary>Der Zustand kann nicht sicher automatisch rekonstruiert werden.</summary>
    RequiresManualReview = 9
}
