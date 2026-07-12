namespace Sasd.MailWorkbench.Domain.Enums;

/// <summary>
/// Legt die Strategie fest, mit der sichtbare Nachrichten geprüft werden.
/// </summary>
public enum ImportMode
{
    /// <summary>
    /// Bekannte Quellschlüssel werden als schnelle Optimierung übersprungen.
    /// </summary>
    Incremental = 0,

    /// <summary>
    /// Jede sichtbare Nachricht wird erneut vollständig gelesen und bytegenau geprüft.
    /// Dies bildet später die Grundlage des Menübefehls <c>Get all</c>.
    /// </summary>
    CompleteVerification = 1
}
