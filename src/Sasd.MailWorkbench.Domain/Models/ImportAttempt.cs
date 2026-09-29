using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Domain.Models;

/// <summary>
/// Persistierbarer Zustand eines einzelnen Importversuchs.
/// </summary>
/// <remarks>
/// Der Datensatz ist absichtlich unveränderlich modelliert. Jeder Zustandswechsel
/// erzeugt mit einem <c>with</c>-Ausdruck eine neue Fassung, die anschließend
/// vom Repository validiert und gespeichert wird. Dadurch bleibt im Code sichtbar,
/// welche Informationen sich bei einem Übergang ändern.
/// </remarks>
public sealed record ImportAttempt
{
    /// <summary>Eindeutige technische ID des Importversuchs.</summary>
    public required string Id { get; init; }

    /// <summary>Logische ID des Mailkontos, zu dem die Nachricht gehört.</summary>
    public required string AccountId { get; init; }

    /// <summary>Quellseitiger Schlüssel, später beispielsweise eine POP3-UIDL.</summary>
    public required string SourceKey { get; init; }

    /// <summary>Aktueller Zustand der wiederanlaufbaren Import-Zustandsmaschine.</summary>
    public required ImportAttemptState State { get; init; }

    /// <summary>Relativer Pfad der vollständig oder teilweise geschriebenen Stagingdatei.</summary>
    public string? StagingRelativePath { get; init; }

    /// <summary>Relativer Pfad der vorgesehenen kanonischen Rohmail-Datei.</summary>
    public string? TargetRelativePath { get; init; }

    /// <summary>Bytegenauer Fingerprint, sobald das Staging abgeschlossen wurde.</summary>
    public MessageFingerprint? Fingerprint { get; init; }

    /// <summary>ID einer bereits vorhandenen kanonischen Nachricht bei einem Duplikat.</summary>
    public string? ExistingMessageId { get; init; }

    /// <summary>UTC-Zeitpunkt, zu dem der Versuch angelegt wurde.</summary>
    public required DateTimeOffset StartedAtUtc { get; init; }

    /// <summary>UTC-Zeitpunkt der letzten Zustandsänderung.</summary>
    public required DateTimeOffset UpdatedAtUtc { get; init; }

    /// <summary>UTC-Zeitpunkt eines terminalen Abschlusses, sofern vorhanden.</summary>
    public DateTimeOffset? CompletedAtUtc { get; init; }

    /// <summary>Anzahl fehlgeschlagener beziehungsweise erneut begonnener Bearbeitungen.</summary>
    public int RetryCount { get; init; }

    /// <summary>Stabiler maschinenlesbarer Fehlercode.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Menschenlesbare technische Fehlerbeschreibung ohne Geheimnisse.</summary>
    public string? ErrorMessage { get; init; }
}
