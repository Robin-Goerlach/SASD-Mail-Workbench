namespace Sasd.MailClient.Domain.Enums;

/// <summary>
/// Differenziert Meldungen aus Parsern und Prozessoren nach ihrer Relevanz.
/// </summary>
public enum ProcessingNoteLevel
{
    Information = 0,
    Warning = 1,
    Error = 2
}
