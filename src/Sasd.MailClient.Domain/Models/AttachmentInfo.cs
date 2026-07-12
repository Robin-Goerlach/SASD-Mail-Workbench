namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Platzhalter fuer Anhangsmetadaten.
/// Der erste Projektwurf nutzt dies noch nicht voll, aber das Nachrichtenmodell ist so
/// bereits fuer spaetere MIME-Erweiterungen vorbereitet.
/// </summary>
public sealed class AttachmentInfo
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long ApproximateSizeInBytes { get; set; }
}
