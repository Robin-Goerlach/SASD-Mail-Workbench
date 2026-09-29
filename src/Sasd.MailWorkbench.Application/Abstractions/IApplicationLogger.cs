namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Kleine Logging-Abstraktion ohne Bindung an eine konkrete Logging-Bibliothek.
/// </summary>
/// <remarks>
/// Die Application-Schicht darf dadurch protokollieren, ohne Serilog, NLog,
/// EventLog oder eine spätere GUI direkt zu referenzieren.
/// </remarks>
public interface IApplicationLogger
{
    /// <summary>Schreibt ein normales Informationsereignis.</summary>
    /// <param name="eventId">Stabile, maschinenlesbare Ereignis-ID.</param>
    /// <param name="message">Menschenlesbare Beschreibung ohne Zugangsdaten.</param>
    void Information(string eventId, string message);

    /// <summary>Schreibt einen nicht fatalen Warnhinweis.</summary>
    /// <param name="eventId">Stabile, maschinenlesbare Ereignis-ID.</param>
    /// <param name="message">Menschenlesbare Beschreibung ohne Zugangsdaten.</param>
    void Warning(string eventId, string message);

    /// <summary>Schreibt einen Fehler.</summary>
    /// <param name="eventId">Stabile, maschinenlesbare Ereignis-ID.</param>
    /// <param name="message">Menschenlesbare Beschreibung ohne Zugangsdaten.</param>
    /// <param name="exception">Optionale technische Ausnahme für Diagnosezwecke.</param>
    void Error(string eventId, string message, Exception? exception = null);
}
