namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Berechnet einen stabilen Fingerprint fuer Rohinhalte, damit Dubletten oder Wiederholungen
/// spaeter besser erkannt werden koennen.
/// </summary>
public interface IMessageFingerprintService
{
    string CreateFingerprint(string rawContent);
}
