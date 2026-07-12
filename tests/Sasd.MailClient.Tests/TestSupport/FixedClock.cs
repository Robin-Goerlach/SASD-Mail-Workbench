using System;
using Sasd.MailClient.Application.Abstractions;

namespace Sasd.MailClient.Tests.TestSupport;

/// <summary>
/// Deterministische Uhr fuer Tests.
/// So bleiben Zeitwerte reproduzierbar.
/// </summary>
public sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; }
}
