using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>Empfängt strukturierte, von Mailinhalten getrennte technische Fehlerdiagnosen.</summary>
public interface IWatcherDiagnosticSink
{
    /// <summary>Meldet einen technischen Fehler, ohne unkontrolliert Mailinhalte zu protokollieren.</summary>
    ValueTask ReportFailureAsync(
        WatcherFailure failure,
        CancellationToken cancellationToken = default);
}
