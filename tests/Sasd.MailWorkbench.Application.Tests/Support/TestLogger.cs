using Sasd.MailWorkbench.Application.Abstractions;

namespace Sasd.MailWorkbench.Application.Tests.Support;

internal sealed class TestLogger : IApplicationLogger
{
    public List<string> Entries { get; } = [];

    public void Information(string eventId, string message) => Entries.Add($"INF:{eventId}:{message}");

    public void Warning(string eventId, string message) => Entries.Add($"WRN:{eventId}:{message}");

    public void Error(string eventId, string message, Exception? exception = null) => Entries.Add($"ERR:{eventId}:{message}");
}
