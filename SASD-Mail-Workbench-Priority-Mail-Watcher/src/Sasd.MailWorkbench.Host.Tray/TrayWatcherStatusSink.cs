using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Host.Tray;

/// <summary>Überträgt den technischen Watcher-Status in die Tray-Oberfläche.</summary>
internal sealed class TrayWatcherStatusSink : IWatcherStatusSink
{
    private readonly TrayUiDispatcher _dispatcher;
    private readonly Action<WatcherStatus> _statusChanged;

    public TrayWatcherStatusSink(
        TrayUiDispatcher dispatcher,
        Action<WatcherStatus> statusChanged)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _statusChanged = statusChanged ?? throw new ArgumentNullException(nameof(statusChanged));
    }

    /// <inheritdoc />
    public ValueTask ReportAsync(
        WatcherStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(status);
        cancellationToken.ThrowIfCancellationRequested();
        _dispatcher.Post(() => _statusChanged(status));
        return ValueTask.CompletedTask;
    }
}
