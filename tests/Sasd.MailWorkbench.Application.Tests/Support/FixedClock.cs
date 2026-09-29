using Sasd.MailWorkbench.Application.Abstractions;

namespace Sasd.MailWorkbench.Application.Tests.Support;

internal sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;

    public DateTimeOffset UtcNow { get; set; }
}
