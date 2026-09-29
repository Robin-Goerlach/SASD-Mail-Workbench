using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Notification.Application.Services;

/// <summary>
/// Orchestriert einen einzelnen Watcher-Prüflauf und verhindert überlappende Ausführungen.
/// </summary>
public sealed class PriorityMailWatcherCoordinator : IDisposable
{
    private readonly IPriorityMailRuleRepository _ruleRepository;
    private readonly IMailObservationSource _mailObservationSource;
    private readonly PriorityMailEvaluationService _evaluationService;
    private readonly IWatcherStatusSink _statusSink;
    private readonly IWatcherDiagnosticSink _diagnosticSink;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _runGate = new(1, 1);
    private bool _disposed;

    /// <summary>Initialisiert einen neuen Koordinator.</summary>
    public PriorityMailWatcherCoordinator(
        IMailObservationSource mailObservationSource,
        IPriorityMailRuleRepository ruleRepository,
        PriorityMailEvaluationService evaluationService,
        IWatcherStatusSink statusSink,
        IWatcherDiagnosticSink diagnosticSink,
        TimeProvider timeProvider)
    {
        _mailObservationSource = mailObservationSource ?? throw new ArgumentNullException(nameof(mailObservationSource));
        _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
        _evaluationService = evaluationService ?? throw new ArgumentNullException(nameof(evaluationService));
        _statusSink = statusSink ?? throw new ArgumentNullException(nameof(statusSink));
        _diagnosticSink = diagnosticSink ?? throw new ArgumentNullException(nameof(diagnosticSink));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>Führt genau einen nicht überlappenden Prüfzyklus aus.</summary>
    public async Task<WatcherRunSummary> RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PriorityMailWatcherCoordinator));
        }

        if (!await _runGate.WaitAsync(0, cancellationToken))
        {
            return WatcherRunSummary.Skipped(_timeProvider.GetUtcNow());
        }

        var startedAtUtc = _timeProvider.GetUtcNow();

        try
        {
            await _statusSink.ReportAsync(
                new WatcherStatus(
                    WatcherOperationalState.Checking,
                    startedAtUtc,
                    "Priority-Mail-Regeln werden geprüft."),
                cancellationToken);

            var activeRules = await _ruleRepository.GetActiveRulesAsync(startedAtUtc, cancellationToken);
            if (activeRules.Count == 0)
            {
                var emptyEvaluation = new PriorityMailEvaluationSummary(0, 0, 0, 0, 0, 0);
                var finishedWithoutRules = _timeProvider.GetUtcNow();

                await _statusSink.ReportAsync(
                    new WatcherStatus(
                        WatcherOperationalState.Idle,
                        finishedWithoutRules,
                        "Keine aktiven Beobachtungsregeln."),
                    cancellationToken);

                return new WatcherRunSummary(
                    startedAtUtc,
                    finishedWithoutRules,
                    0,
                    emptyEvaluation,
                    false);
            }

            var observations = await _mailObservationSource.GetNewObservationsAsync(cancellationToken);
            var evaluation = await _evaluationService.EvaluateAsync(
                observations,
                activeRules,
                cancellationToken);
            var finishedAtUtc = _timeProvider.GetUtcNow();

            await _statusSink.ReportAsync(
                new WatcherStatus(
                    WatcherOperationalState.Idle,
                    finishedAtUtc,
                    $"Prüfung beendet: {evaluation.PublishedNotifications} Benachrichtigung(en)."),
                cancellationToken);

            return new WatcherRunSummary(
                startedAtUtc,
                finishedAtUtc,
                activeRules.Count,
                evaluation,
                false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var failedAtUtc = _timeProvider.GetUtcNow();
            await _diagnosticSink.ReportFailureAsync(
                new WatcherFailure(
                    failedAtUtc,
                    "watcher-run",
                    "watcher-run-failed",
                    exception.GetType().FullName ?? exception.GetType().Name),
                CancellationToken.None);
            await _statusSink.ReportAsync(
                new WatcherStatus(
                    WatcherOperationalState.Faulted,
                    failedAtUtc,
                    "Die letzte Prüfung ist fehlgeschlagen.",
                    "watcher-run-failed"),
                CancellationToken.None);
            throw;
        }
        finally
        {
            _runGate.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _runGate.Dispose();
    }
}
