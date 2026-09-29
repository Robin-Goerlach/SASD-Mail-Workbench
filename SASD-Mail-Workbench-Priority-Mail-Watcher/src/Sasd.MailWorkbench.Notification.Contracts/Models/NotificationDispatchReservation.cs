namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Beschreibt eine atomar angelegte Benachrichtigungsreservierung.</summary>
public sealed record NotificationDispatchReservation(
    Guid ReservationId,
    NotificationDispatchKey Key,
    DateTimeOffset ReservedAtUtc);
