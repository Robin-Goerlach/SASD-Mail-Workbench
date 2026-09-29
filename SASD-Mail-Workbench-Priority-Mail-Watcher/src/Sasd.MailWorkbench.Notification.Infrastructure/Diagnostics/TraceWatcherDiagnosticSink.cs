using System.Diagnostics;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Infrastructure.Diagnostics;

/// <summary>Schreibt strukturierte Bootstrap-Diagnosen in die .NET-Trace-Infrastruktur.</summary>
public sealed class TraceWatcherDiagnosticSink : IWatcherDiagnosticSink
{
    /// <inheritdoc />
    public ValueTask ReportFailureAsync(
        WatcherFailure failure,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(failure);
        cancellationToken.ThrowIfCancellationRequested();

        Trace.TraceError(
            "PriorityMailWatcher failure: Stage={0}; Code={1}; ExceptionType={2}; RuleId={3}; MessageIdentity={4}",
            failure.Stage,
            failure.FailureCode,
            failure.ExceptionType,
            failure.RuleId,
            failure.StableMessageId);

        return ValueTask.CompletedTask;
    }
}
