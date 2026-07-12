using System;
using Sasd.MailClient.Application.Abstractions;

namespace Sasd.MailClient.Infrastructure.Services;

/// <summary>
/// Standard-Implementierung fuer Zeitangaben.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
