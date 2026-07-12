namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Abstrakte UTC-Zeitquelle für reproduzierbare Tests.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Liefert den aktuellen UTC-Zeitpunkt.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
