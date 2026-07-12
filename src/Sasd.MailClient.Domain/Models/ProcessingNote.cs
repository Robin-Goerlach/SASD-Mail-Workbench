using Sasd.MailClient.Domain.Enums;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Beschreibt eine einzelne Diagnose-, Warn- oder Fehlermeldung aus Parser oder Prozessoren.
/// </summary>
public sealed class ProcessingNote
{
    public ProcessingNoteLevel Level { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
