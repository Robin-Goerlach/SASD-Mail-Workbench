using System;
using Sasd.MailClient.Application.Abstractions;

namespace Sasd.MailClient.Infrastructure.Logging;

/// <summary>
/// Sehr einfacher Logger fuer den Start.
/// Spaeter kann hier problemlos eine strukturierte Logging-Bibliothek hinterlegt werden.
/// </summary>
public sealed class ConsoleApplicationLogger : IApplicationLogger
{
    public void LogInformation(string message)
    {
        Write("INFO ", message, ConsoleColor.Green);
    }

    public void LogWarning(string message)
    {
        Write("WARN ", message, ConsoleColor.Yellow);
    }

    public void LogError(string message, Exception? exception = null)
    {
        string completeMessage = exception is null
            ? message
            : $"{message}{Environment.NewLine}{exception}";

        Write("ERROR", completeMessage, ConsoleColor.Red);
    }

    private static void Write(string prefix, string message, ConsoleColor color)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{prefix}] {message}");
        Console.ForegroundColor = previousColor;
    }
}
