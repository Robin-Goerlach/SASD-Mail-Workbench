using System.Diagnostics;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Infrastructure.Diagnostics;

/// <summary>Schreibt Watcher-Statusänderungen in die .NET-Trace-Infrastruktur.</summary>
public sealed class TraceWatcherStatusSink : IWatcherStatusSink
{
    /// <inheritdoc />
    public ValueTask ReportAsync(
        WatcherStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(status);
        cancellationToken.ThrowIfCancellationRequested();
        Trace.TraceInformation(
            "PriorityMailWatcher status: State={0}; Message={1}; DiagnosticCode={2}",
            status.State,
            status.Message,
            status.DiagnosticCode);
        return ValueTask.CompletedTask;
    }
}
