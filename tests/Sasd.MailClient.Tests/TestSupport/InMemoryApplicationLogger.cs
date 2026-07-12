using System;
using System.Collections.Generic;
using Sasd.MailClient.Application.Abstractions;

namespace Sasd.MailClient.Tests.TestSupport;

/// <summary>
/// Kleiner Logger fuer Tests, damit Testfaelle Meldungen sammeln koennen,
/// ohne auf Konsole oder Dateisystem angewiesen zu sein.
/// </summary>
public sealed class InMemoryApplicationLogger : IApplicationLogger
{
    public List<string> InformationMessages { get; } = new List<string>();

    public List<string> WarningMessages { get; } = new List<string>();

    public List<string> ErrorMessages { get; } = new List<string>();

    public void LogInformation(string message)
    {
        InformationMessages.Add(message);
    }

    public void LogWarning(string message)
    {
        WarningMessages.Add(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        ErrorMessages.Add(exception is null ? message : $"{message}\n{exception}");
    }
}
