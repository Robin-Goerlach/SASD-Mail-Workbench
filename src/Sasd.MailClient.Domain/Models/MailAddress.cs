namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Ein bewusst einfaches Adressmodell fuer Absender, Empfaenger und Reply-To-Informationen.
/// Fuer den Projektstart ist dies ausreichend und spaeter leicht erweiterbar.
/// </summary>
public sealed class MailAddress
{
    public string DisplayName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Gibt eine gut lesbare Darstellung fuer Logging oder Konsolenausgaben zurueck.
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(DisplayName)
            ? Address
            : $"{DisplayName} <{Address}>";
    }
}
