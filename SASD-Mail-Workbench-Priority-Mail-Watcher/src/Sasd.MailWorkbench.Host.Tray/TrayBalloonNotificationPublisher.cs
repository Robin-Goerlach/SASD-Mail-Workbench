using System.Windows.Forms;

using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Host.Tray;

/// <summary>
/// Übergangsadapter für sichtbare Benachrichtigungen über das Tray-Icon.
/// </summary>
/// <remarks>
/// Dieser Adapter ist absichtlich klein und austauschbar. Eine spätere Windows-App-Notification-
/// Implementierung ersetzt ihn, ohne Application oder Domain zu verändern.
/// </remarks>
internal sealed class TrayBalloonNotificationPublisher : IUserNotificationPublisher
{
    private readonly NotifyIcon _notifyIcon;
    private readonly TrayUiDispatcher _dispatcher;

    public TrayBalloonNotificationPublisher(NotifyIcon notifyIcon, TrayUiDispatcher dispatcher)
    {
        _notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <inheritdoc />
    public ValueTask PublishAsync(
        PriorityMailNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();

        _dispatcher.Post(() =>
        {
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            _notifyIcon.BalloonTipTitle = "Wichtige Nachricht erkannt";
            _notifyIcon.BalloonTipText = $"{notification.RuleName}: {notification.Subject}";
            _notifyIcon.ShowBalloonTip(10_000);
        });

        return ValueTask.CompletedTask;
    }
}
