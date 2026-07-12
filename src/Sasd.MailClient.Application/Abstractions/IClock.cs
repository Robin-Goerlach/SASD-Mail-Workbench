using System;

namespace Sasd.MailClient.Application.Abstractions;

/// <summary>
/// Kleines Zeit-Interface, damit Zeitpunkte spaeter einfacher testbar oder simulierbar werden.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
