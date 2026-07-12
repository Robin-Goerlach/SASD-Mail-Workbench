namespace Sasd.MailClient.Domain.Enums;

/// <summary>
/// Beschreibt den nachvollziehbaren Lebenszyklus einer Nachricht innerhalb des lokalen Systems.
/// Diese Zustandswerte sind fuer Diagnose, Wiederholungslaeufe und spaetere GUI-Statusanzeigen wichtig.
/// </summary>
public enum MessageProcessingState
{
    Discovered = 0,
    Fetched = 1,
    RawStored = 2,
    Parsed = 3,
    ParseFailed = 4,
    Processed = 5,
    ProcessingFailed = 6,
    RequiresManualReview = 7
}
