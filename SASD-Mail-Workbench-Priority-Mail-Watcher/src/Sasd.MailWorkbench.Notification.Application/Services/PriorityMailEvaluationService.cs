using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;
using Sasd.MailWorkbench.Notification.Domain.Matching;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Application.Services;

/// <summary>
/// Bewertet neue Mailbeobachtungen, reserviert Benachrichtigungen und veröffentlicht Treffer.
/// </summary>
public sealed class PriorityMailEvaluationService
{
    private readonly PriorityMailRuleMatcher _matcher;
    private readonly INotificationDispatchStore _dispatchStore;
    private readonly IUserNotificationPublisher _notificationPublisher;
    private readonly IWatcherDiagnosticSink _diagnosticSink;
    private readonly TimeProvider _timeProvider;

    /// <summary>Initialisiert den Anwendungsdienst mit allen erforderlichen Ports.</summary>
    public PriorityMailEvaluationService(
        PriorityMailRuleMatcher matcher,
        INotificationDispatchStore dispatchStore,
        IUserNotificationPublisher notificationPublisher,
        IWatcherDiagnosticSink diagnosticSink,
        TimeProvider timeProvider)
    {
        _matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
        _dispatchStore = dispatchStore ?? throw new ArgumentNullException(nameof(dispatchStore));
        _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
        _diagnosticSink = diagnosticSink ?? throw new ArgumentNullException(nameof(diagnosticSink));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>Bewertet einen unveränderlichen Snapshot von Beobachtungen und Regeln.</summary>
    public async Task<PriorityMailEvaluationSummary> EvaluateAsync(
        IReadOnlyList<MailObservation> observations,
        IReadOnlyList<PriorityMailRule> rules,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(observations);
        ArgumentNullException.ThrowIfNull(rules);

        var evaluatedCombinations = 0;
        var matchingCombinations = 0;
        var publishedNotifications = 0;
        var suppressedDuplicates = 0;
        var failedNotifications = 0;
        var evaluationTimeUtc = _timeProvider.GetUtcNow();

        foreach (var observation in observations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var rule in rules)
            {
                cancellationToken.ThrowIfCancellationRequested();
                evaluatedCombinations++;

                var match = _matcher.Match(rule, observation, evaluationTimeUtc);
                if (!match.IsMatch)
                {
                    continue;
                }

                matchingCombinations++;
                var dispatchKey = new NotificationDispatchKey(
                    rule.Id,
                    observation.AccountId,
                    observation.StableMessageId);
                var reservation = await _dispatchStore.TryReserveAsync(
                    dispatchKey,
                    _timeProvider.GetUtcNow(),
                    cancellationToken);

                if (reservation is null)
                {
                    suppressedDuplicates++;
                    continue;
                }

                var notification = new PriorityMailNotification(
                    rule.Id,
                    rule.Name,
                    observation.AccountId,
                    observation.StableMessageId,
                    observation.FromAddress,
                    observation.Subject,
                    observation.ReceivedAtUtc,
                    match.ReasonCodes);

                try
                {
                    await _notificationPublisher.PublishAsync(notification, cancellationToken);
                    publishedNotifications++;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    await _dispatchStore.MarkFailedAsync(
                        reservation.ReservationId,
                        _timeProvider.GetUtcNow(),
                        "notification-cancelled",
                        CancellationToken.None);
                    throw;
                }
                catch (Exception exception)
                {
                    failedNotifications++;
                    await _dispatchStore.MarkFailedAsync(
                        reservation.ReservationId,
                        _timeProvider.GetUtcNow(),
                        "notification-publish-failed",
                        CancellationToken.None);
                    await ReportFailureAsync(
                        exception,
                        "publish-notification",
                        "notification-publish-failed",
                        rule.Id,
                        observation.StableMessageId);
                    continue;
                }

                try
                {
                    // Nach sichtbarer Zustellung wird der Zustellstatus auch bei zwischenzeitlicher
                    // Benutzerabmeldung ohne den Lauf-CancellationToken abgeschlossen.
                    await _dispatchStore.MarkDeliveredAsync(
                        reservation.ReservationId,
                        _timeProvider.GetUtcNow(),
                        CancellationToken.None);
                }
                catch (Exception exception)
                {
                    // Die Reservierung wird absichtlich nicht freigegeben: Eine Wiederholung könnte
                    // eine bereits sichtbare Benachrichtigung duplizieren.
                    failedNotifications++;
                    await ReportFailureAsync(
                        exception,
                        "persist-notification-state",
                        "notification-state-uncertain",
                        rule.Id,
                        observation.StableMessageId);
                }
            }
        }

        return new PriorityMailEvaluationSummary(
            observations.Count,
            evaluatedCombinations,
            matchingCombinations,
            publishedNotifications,
            suppressedDuplicates,
            failedNotifications);
    }
    private ValueTask ReportFailureAsync(
        Exception exception,
        string stage,
        string failureCode,
        Guid ruleId,
        string stableMessageId) =>
        _diagnosticSink.ReportFailureAsync(
            new WatcherFailure(
                _timeProvider.GetUtcNow(),
                stage,
                failureCode,
                exception.GetType().FullName ?? exception.GetType().Name,
                ruleId,
                stableMessageId),
            CancellationToken.None);

}
