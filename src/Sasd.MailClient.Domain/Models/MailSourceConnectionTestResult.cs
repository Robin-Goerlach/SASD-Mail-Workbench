namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Ergebnis einer Quellverbindung. Diese Trennung hilft spaeter, technische Fehlermeldungen
/// sauber und benutzerfreundlich aufzubereiten.
/// </summary>
public sealed class MailSourceConnectionTestResult
{
    public bool Success { get; set; }

    public string TechnicalMessage { get; set; } = string.Empty;
}
