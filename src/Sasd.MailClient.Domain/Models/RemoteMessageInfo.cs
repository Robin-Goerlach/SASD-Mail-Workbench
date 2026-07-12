namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Beschreibt eine Nachricht, die in einer externen Quelle entdeckt wurde, aber noch
/// nicht vollstaendig lokal vorliegt.
/// </summary>
public sealed class RemoteMessageInfo
{
    public string ExternalKey { get; set; } = string.Empty;

    public string SourceIdentifier { get; set; } = string.Empty;

    public long ApproximateSizeInBytes { get; set; }
}
