using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>
/// Reserviert und protokolliert Benachrichtigungszustände zur Vermeidung doppelter Meldungen.
/// </summary>
public interface INotificationDispatchStore
{
    /// <summary>
    /// Reserviert eine Regel-/Nachrichtenkombination atomar. Existiert bereits eine Reservierung,
    /// wird <see langword="null"/> zurückgegeben.
    /// </summary>
    ValueTask<NotificationDispatchReservation?> TryReserveAsync(
        NotificationDispatchKey key,
        DateTimeOffset reservedAtUtc,
        CancellationToken cancellationToken = default);

    /// <summary>Markiert eine zuvor reservierte Benachrichtigung als erfolgreich zugestellt.</summary>
    ValueTask MarkDeliveredAsync(
        Guid reservationId,
        DateTimeOffset deliveredAtUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Markiert eine Reservierung als fehlgeschlagen und gibt sie für einen späteren Retry frei.
    /// </summary>
    ValueTask MarkFailedAsync(
        Guid reservationId,
        DateTimeOffset failedAtUtc,
        string failureCode,
        CancellationToken cancellationToken = default);
}
