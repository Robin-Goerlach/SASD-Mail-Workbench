namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Enthält den Inhalt einer bereits aus der Quelle geladenen Nachricht.
/// Im ersten Startwurf ist dies einfach der Rohtext der Nachricht.
/// </summary>
public sealed class RemoteMessageContent
{
    public RemoteMessageInfo RemoteMessage { get; set; } = new RemoteMessageInfo();

    public string RawContent { get; set; } = string.Empty;
}
