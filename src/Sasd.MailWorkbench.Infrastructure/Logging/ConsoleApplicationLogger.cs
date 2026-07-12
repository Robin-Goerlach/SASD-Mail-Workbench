using Sasd.MailWorkbench.Application.Abstractions;

namespace Sasd.MailWorkbench.Infrastructure.Logging;

/// <summary>
/// Einfache Konsolenimplementierung für Demo und lokale Entwicklung.
/// </summary>
/// <remarks>
/// Eine spätere Desktop-Anwendung kann dieselbe Application-Schicht mit einem
/// strukturierten Datei- oder UI-Logger betreiben. Zugangsdaten oder Mailtexte
/// dürfen auch dann nicht ungefiltert protokolliert werden.
/// </remarks>
public sealed class ConsoleApplicationLogger : IApplicationLogger
{
    /// <inheritdoc />
    public void Information(string eventId, string message) =>
        Write("INFO", eventId, message, ConsoleColor.Gray);

    /// <inheritdoc />
    public void Warning(string eventId, string message) =>
        Write("WARN", eventId, message, ConsoleColor.Yellow);

    /// <inheritdoc />
    public void Error(string eventId, string message, Exception? exception = null)
    {
        string details = exception is null ? message : $"{message} | {exception.GetType().Name}: {exception.Message}";
        Write("ERROR", eventId, details, ConsoleColor.Red);
    }

    private static void Write(string level, string eventId, string message, ConsoleColor color)
    {
        ConsoleColor previous = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTimeOffset.UtcNow:O} [{level}] {eventId}: {message}");
        }
        finally
        {
            Console.ForegroundColor = previous;
        }
    }
}
