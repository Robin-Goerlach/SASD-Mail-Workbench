using System.Windows.Forms;

using Sasd.MailWorkbench.Notification.Application.Services;
using Sasd.MailWorkbench.Notification.Domain.Matching;
using Sasd.MailWorkbench.Notification.Infrastructure.Bootstrap;
using Sasd.MailWorkbench.Notification.Infrastructure.Diagnostics;

namespace Sasd.MailWorkbench.Host.Tray;

/// <summary>
/// Erstellt den gegenwärtigen Bootstrap-Objektgraphen des Tray-Prozesses.
/// </summary>
/// <remarks>
/// Die Composition Root ist der einzige Ort, an dem konkrete Adapter zusammengeführt werden.
/// Produktive Mail- und SQLite-Adapter werden später hier injiziert; der fachliche Kern bleibt
/// unverändert.
/// </remarks>
internal static class WatcherCompositionRoot
{
    public static PriorityMailWatcherCoordinator Create(
        NotifyIcon notifyIcon,
        TrayUiDispatcher dispatcher,
        Action<Sasd.MailWorkbench.Notification.Contracts.Models.WatcherStatus> statusChanged)
    {
        ArgumentNullException.ThrowIfNull(notifyIcon);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(statusChanged);

        var diagnosticSink = new TraceWatcherDiagnosticSink();
        var statusSink = new TrayWatcherStatusSink(dispatcher, statusChanged);
        var notificationPublisher = new TrayBalloonNotificationPublisher(notifyIcon, dispatcher);

        var evaluationService = new PriorityMailEvaluationService(
            new PriorityMailRuleMatcher(),
            new InMemoryNotificationDispatchStore(),
            notificationPublisher,
            diagnosticSink,
            TimeProvider.System);

        return new PriorityMailWatcherCoordinator(
            new EmptyMailObservationSource(),
            new InMemoryPriorityMailRuleRepository(),
            evaluationService,
            statusSink,
            diagnosticSink,
            TimeProvider.System);
    }
}
