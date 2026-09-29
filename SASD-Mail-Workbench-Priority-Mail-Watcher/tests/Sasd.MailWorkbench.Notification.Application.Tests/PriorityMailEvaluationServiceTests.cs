using NUnit.Framework;
using Sasd.MailWorkbench.Notification.Application.Services;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;
using Sasd.MailWorkbench.Notification.Domain.Matching;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Application.Tests;

[TestFixture]
public sealed class PriorityMailEvaluationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 24, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task EvaluateAsync_SameRuleAndMessageTwice_PublishesOnlyOnce()
    {
        var dispatchStore = new TestDispatchStore();
        var publisher = new RecordingPublisher();
        var service = CreateService(dispatchStore, publisher);
        var rule = CreateRule();
        var observation = CreateObservation();

        var first = await service.EvaluateAsync([observation], [rule]);
        var second = await service.EvaluateAsync([observation], [rule]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.PublishedNotifications, Is.EqualTo(1));
            Assert.That(second.SuppressedDuplicates, Is.EqualTo(1));
            Assert.That(publisher.Notifications, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public async Task EvaluateAsync_SameTechnicalMessageIdInDifferentAccounts_PublishesForEachAccount()
    {
        var dispatchStore = new TestDispatchStore();
        var publisher = new RecordingPublisher();
        var service = CreateService(dispatchStore, publisher);
        var rule = CreateRule();
        var firstAccountObservation = CreateObservation();
        var secondAccountObservation = new MailObservation(
            "account-2",
            firstAccountObservation.StableMessageId,
            firstAccountObservation.FromAddress,
            null,
            firstAccountObservation.Subject,
            firstAccountObservation.SearchText,
            firstAccountObservation.HasAttachments,
            firstAccountObservation.ReceivedAtUtc);

        var result = await service.EvaluateAsync(
            [firstAccountObservation, secondAccountObservation],
            [rule]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PublishedNotifications, Is.EqualTo(2));
            Assert.That(result.SuppressedDuplicates, Is.Zero);
            Assert.That(publisher.Notifications.Select(item => item.AccountId), Is.EquivalentTo(new[] { "account-1", "account-2" }));
        }
    }

    [Test]
    public async Task EvaluateAsync_NonMatchingMessage_DoesNotReserveOrPublish()
    {
        var dispatchStore = new TestDispatchStore();
        var publisher = new RecordingPublisher();
        var service = CreateService(dispatchStore, publisher);
        var observation = new MailObservation(
            "account-1",
            "inbox:42:1002",
            "newsletter@example.net",
            null,
            "Newsletter",
            string.Empty,
            false,
            Now);

        var result = await service.EvaluateAsync([observation], [CreateRule()]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PublishedNotifications, Is.Zero);
            Assert.That(dispatchStore.Reservations, Is.Zero);
            Assert.That(publisher.Notifications, Is.Empty);
        }
    }

    private static PriorityMailEvaluationService CreateService(
        INotificationDispatchStore dispatchStore,
        IUserNotificationPublisher publisher) =>
        new(
            new PriorityMailRuleMatcher(),
            dispatchStore,
            publisher,
            new NullDiagnosticSink(),
            new FixedTimeProvider(Now));

    private static PriorityMailRule CreateRule() =>
        new(
            Guid.Parse("9fd8062e-d2e6-487e-8a5d-ae4b214c5428"),
            "Support beobachten",
            new PriorityMailRuleCriteria(senderDomains: ["example.com"]));

    private static MailObservation CreateObservation() =>
        new(
            "account-1",
            "inbox:42:1001",
            "support@example.com",
            null,
            "Antwort auf Ihr Ticket",
            string.Empty,
            false,
            Now);

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class RecordingPublisher : IUserNotificationPublisher
    {
        public List<PriorityMailNotification> Notifications { get; } = [];

        public ValueTask PublishAsync(
            PriorityMailNotification notification,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Notifications.Add(notification);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestDispatchStore : INotificationDispatchStore
    {
        private readonly HashSet<NotificationDispatchKey> _delivered = [];
        private readonly Dictionary<Guid, NotificationDispatchKey> _reservations = [];

        public int Reservations { get; private set; }

        public ValueTask<NotificationDispatchReservation?> TryReserveAsync(
            NotificationDispatchKey key,
            DateTimeOffset reservedAtUtc,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_delivered.Contains(key) || _reservations.ContainsValue(key))
            {
                return ValueTask.FromResult<NotificationDispatchReservation?>(null);
            }

            var reservation = new NotificationDispatchReservation(Guid.NewGuid(), key, reservedAtUtc);
            _reservations.Add(reservation.ReservationId, key);
            Reservations++;
            return ValueTask.FromResult<NotificationDispatchReservation?>(reservation);
        }

        public ValueTask MarkDeliveredAsync(
            Guid reservationId,
            DateTimeOffset deliveredAtUtc,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = deliveredAtUtc;
            var key = _reservations[reservationId];
            _reservations.Remove(reservationId);
            _delivered.Add(key);
            return ValueTask.CompletedTask;
        }

        public ValueTask MarkFailedAsync(
            Guid reservationId,
            DateTimeOffset failedAtUtc,
            string failureCode,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = failedAtUtc;
            _ = failureCode;
            _reservations.Remove(reservationId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class NullDiagnosticSink : IWatcherDiagnosticSink
    {
        public ValueTask ReportFailureAsync(
            WatcherFailure failure,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }
}
