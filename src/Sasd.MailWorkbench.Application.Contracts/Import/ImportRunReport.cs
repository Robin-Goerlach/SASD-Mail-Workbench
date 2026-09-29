using Sasd.MailWorkbench.Domain.Enums;

namespace Sasd.MailWorkbench.Application.Contracts.Import;

/// <summary>
/// Fasst einen vollständigen Importlauf zusammen.
/// </summary>
/// <remarks>
/// Dieses Objekt ist bewusst ein reiner Transportvertrag. Es enthält keine
/// Datenbank- oder Dateisystemlogik und kann später unverändert von einer
/// WinForms-, WPF- oder MAUI-Oberfläche angezeigt werden.
/// </remarks>
public sealed class ImportRunReport
{
    private readonly List<ImportRunError> _errors = [];

    /// <summary>
    /// Zeitpunkt, zu dem der Importlauf begonnen hat.
    /// </summary>
    public required DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>
    /// Zeitpunkt, zu dem der Lauf abgeschlossen oder abgebrochen wurde.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Fachliches Gesamtergebnis des Laufs.
    /// </summary>
    public ImportRunOutcome Outcome { get; private set; }

    /// <summary>
    /// Anzahl der von der Quelle sichtbaren Nachrichtenobjekte.
    /// </summary>
    public int Discovered { get; internal set; }

    /// <summary>
    /// Anzahl der im inkrementellen Modus anhand des Quellschlüssels übersprungenen Objekte.
    /// </summary>
    public int SkippedKnownSourceKeys { get; internal set; }

    /// <summary>
    /// Anzahl neu in den kanonischen Rohmail-Speicher übernommener Nachrichten.
    /// </summary>
    public int NewlyStored { get; internal set; }

    /// <summary>
    /// Anzahl bytegenau erkannter Duplikate.
    /// </summary>
    public int ExactDuplicates { get; internal set; }

    /// <summary>
    /// Anzahl der Nachrichtenobjekte, deren Verarbeitung fehlgeschlagen ist.
    /// </summary>
    public int Failed { get; internal set; }

    /// <summary>
    /// Erklärbare Fehler einzelner Nachrichtenobjekte.
    /// </summary>
    public IReadOnlyList<ImportRunError> Errors => _errors;

    /// <summary>
    /// Fügt einen Objektfehler hinzu und erhöht gleichzeitig den Fehlerzähler.
    /// </summary>
    /// <param name="error">Der für Benutzer und Diagnose verständliche Fehler.</param>
    public void AddError(ImportRunError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _errors.Add(error);
        Failed++;
    }

    /// <summary>
    /// Markiert den Bericht als beendet.
    /// </summary>
    /// <param name="completedAtUtc">UTC-Zeitpunkt des Abschlusses.</param>
    /// <param name="outcome">Fachliches Gesamtergebnis.</param>
    public void Complete(DateTimeOffset completedAtUtc, ImportRunOutcome outcome)
    {
        CompletedAtUtc = completedAtUtc;
        Outcome = outcome;
    }
}
