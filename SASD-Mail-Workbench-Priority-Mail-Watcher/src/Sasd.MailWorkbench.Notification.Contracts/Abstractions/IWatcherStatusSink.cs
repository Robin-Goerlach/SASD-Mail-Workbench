using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>Empfängt nichtpersistente Laufzeitstatusänderungen des Watchers.</summary>
public interface IWatcherStatusSink
{
    /// <summary>Meldet einen neuen Statussnapshot.</summary>
    ValueTask ReportAsync(
        WatcherStatus status,
        CancellationToken cancellationToken = default);
}
