using Sasd.MailWorkbench.Application.Abstractions;

namespace Sasd.MailWorkbench.Infrastructure.Services;

/// <summary>
/// Produktive UTC-Zeitquelle auf Basis der Systemuhr.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
