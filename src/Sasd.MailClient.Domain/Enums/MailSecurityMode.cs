namespace Sasd.MailClient.Domain.Enums;

/// <summary>
/// Beschreibt, wie eine spaetere echte Mailquelle die Verbindung absichern soll.
/// Die Demo-Mailquelle benoetigt diesen Wert noch nicht direkt, aber das Domainmodell
/// soll ihn bereits enthalten, damit Konten spaeter ohne Umbau erweitert werden koennen.
/// </summary>
public enum MailSecurityMode
{
    None = 0,
    SslTls = 1,
    StartTls = 2
}
