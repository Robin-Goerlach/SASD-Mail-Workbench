using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>Veröffentlicht eine fachlich bestätigte Benachrichtigung im Benutzerkontext.</summary>
public interface IUserNotificationPublisher
{
    /// <summary>Veröffentlicht die angegebene Benachrichtigung.</summary>
    /// <remarks>
    /// Die Implementierung muss eine Ausnahme nur dann auslösen, wenn keine erfolgreiche,
    /// benutzersichtbare Zustellung bestätigt werden kann. Nach erfolgreicher Rückkehr darf die
    /// Application die Benachrichtigung als zugestellt persistieren.
    /// </remarks>
    ValueTask PublishAsync(
        PriorityMailNotification notification,
        CancellationToken cancellationToken = default);
}
