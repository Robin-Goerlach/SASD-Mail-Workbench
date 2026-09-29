using System.Collections.Concurrent;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Infrastructure.Bootstrap;

/// <summary>
/// Prozesslokaler Dispatch-Store für Bootstrap und Tests des Tray-Lifecycles.
/// </summary>
/// <remarks>
/// Diese Implementierung verhindert Duplikate nur während eines Prozesslaufs. Produktiv wird sie
/// durch einen transaktionalen SQLite-Adapter ersetzt, der Prozessneustarts und Parallelzugriffe
/// von Workbench und Watcher berücksichtigt.
/// </remarks>
public sealed class InMemoryNotificationDispatchStore : INotificationDispatchStore
{
    private readonly ConcurrentDictionary<NotificationDispatchKey, DispatchEntry> _entries = new();
    private readonly ConcurrentDictionary<Guid, NotificationDispatchKey> _reservationKeys = new();

    /// <inheritdoc />
    public ValueTask<NotificationDispatchReservation?> TryReserveAsync(
        NotificationDispatchKey key,
        DateTimeOffset reservedAtUtc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateKey(key);

        var reservation = new NotificationDispatchReservation(Guid.NewGuid(), key, reservedAtUtc);
        var entry = new DispatchEntry(reservation, DispatchState.Reserved);

        if (!_entries.TryAdd(key, entry))
        {
            return ValueTask.FromResult<NotificationDispatchReservation?>(null);
        }

        _reservationKeys[reservation.ReservationId] = key;
        return ValueTask.FromResult<NotificationDispatchReservation?>(reservation);
    }

    /// <inheritdoc />
    public ValueTask MarkDeliveredAsync(
        Guid reservationId,
        DateTimeOffset deliveredAtUtc,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_reservationKeys.TryGetValue(reservationId, out var key) ||
            !_entries.TryGetValue(key, out var entry))
        {
            throw new InvalidOperationException("Die Benachrichtigungsreservierung ist unbekannt.");
        }

        _entries[key] = entry with { State = DispatchState.Delivered, CompletedAtUtc = deliveredAtUtc };
        _reservationKeys.TryRemove(reservationId, out _);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask MarkFailedAsync(
        Guid reservationId,
        DateTimeOffset failedAtUtc,
        string failureCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = failedAtUtc;

        if (string.IsNullOrWhiteSpace(failureCode))
        {
            throw new ArgumentException("Der Fehlercode darf nicht leer sein.", nameof(failureCode));
        }

        if (_reservationKeys.TryRemove(reservationId, out var key))
        {
            _entries.TryRemove(key, out _);
        }

        return ValueTask.CompletedTask;
    }

    private static void ValidateKey(NotificationDispatchKey key)
    {
        if (key.RuleId == Guid.Empty)
        {
            throw new ArgumentException("Die Regel-ID darf nicht leer sein.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(key.AccountId))
        {
            throw new ArgumentException("Die Konto-ID darf nicht leer sein.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(key.StableMessageId))
        {
            throw new ArgumentException("Die Nachrichtenidentität darf nicht leer sein.", nameof(key));
        }
    }

    private sealed record DispatchEntry(
        NotificationDispatchReservation Reservation,
        DispatchState State,
        DateTimeOffset? CompletedAtUtc = null);

    private enum DispatchState
    {
        Reserved,
        Delivered
    }
}
