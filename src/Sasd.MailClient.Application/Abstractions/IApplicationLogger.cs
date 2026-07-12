using System;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Sehr kleine Logging-Abstraktion fuer den Projektstart.
/// So bleibt der Kern frei von direkter Bibliotheksbindung.
/// </summary>
public interface IApplicationLogger
{
    void LogInformation(string message);

    void LogWarning(string message);

    void LogError(string message, Exception? exception = null);
}
