using NUnit.Framework;
using Sasd.MailWorkbench.Notification.Application.Services;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;
using Sasd.MailWorkbench.Notification.Domain.Matching;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Application.Tests;

[TestFixture]
public sealed class PriorityMailWatcherCoordinatorTests
{
    [Test]
    public async Task RunOnceAsync_NoActiveRules_DoesNotReadMailSource()
    {
        var source = new CountingSource();
        var statusSink = new RecordingStatusSink();
        using var coordinator = new PriorityMailWatcherCoordinator(
            source,
            new EmptyRuleRepository(),
            CreateEvaluationService(),
            statusSink,
            new NullDiagnosticSink(),
            TimeProvider.System);

        var result = await coordinator.RunOnceAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.ReadCount, Is.Zero);
            Assert.That(result.ActiveRules, Is.Zero);
            Assert.That(statusSink.Statuses.Last().State, Is.EqualTo(WatcherOperationalState.Idle));
        }
    }

    private static PriorityMailEvaluationService CreateEvaluationService() =>
        new(
            new PriorityMailRuleMatcher(),
            new NeverReserveStore(),
            new NullPublisher(),
            new NullDiagnosticSink(),
            TimeProvider.System);

    private sealed class CountingSource : IMailObservationSource
    {
        public int ReadCount { get; private set; }

        public Task<IReadOnlyList<MailObservation>> GetNewObservationsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            return Task.FromResult<IReadOnlyList<MailObservation>>(Array.Empty<MailObservation>());
        }
    }

    private sealed class EmptyRuleRepository : IPriorityMailRuleRepository
    {
        public Task<IReadOnlyList<PriorityMailRule>> GetActiveRulesAsync(
            DateTimeOffset utcNow,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<PriorityMailRule>>(Array.Empty<PriorityMailRule>());
        }
    }

    private sealed class RecordingStatusSink : IWatcherStatusSink
    {
        public List<WatcherStatus> Statuses { get; } = [];

        public ValueTask ReportAsync(
            WatcherStatus status,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Statuses.Add(status);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class NeverReserveStore : INotificationDispatchStore
    {
        public ValueTask<NotificationDispatchReservation?> TryReserveAsync(
            NotificationDispatchKey key,
            DateTimeOffset reservedAtUtc,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<NotificationDispatchReservation?>(null);

        public ValueTask MarkDeliveredAsync(
            Guid reservationId,
            DateTimeOffset deliveredAtUtc,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask MarkFailedAsync(
            Guid reservationId,
            DateTimeOffset failedAtUtc,
            string failureCode,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }

    private sealed class NullPublisher : IUserNotificationPublisher
    {
        public ValueTask PublishAsync(
            PriorityMailNotification notification,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }

    private sealed class NullDiagnosticSink : IWatcherDiagnosticSink
    {
        public ValueTask ReportFailureAsync(
            WatcherFailure failure,
            CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }
}
