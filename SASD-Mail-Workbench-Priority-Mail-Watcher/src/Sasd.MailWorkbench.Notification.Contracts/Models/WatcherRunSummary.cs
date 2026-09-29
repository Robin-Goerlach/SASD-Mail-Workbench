namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Fasst einen einzelnen, vollständig beendeten Watcher-Prüflauf zusammen.</summary>
public sealed record WatcherRunSummary(
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    int ActiveRules,
    PriorityMailEvaluationSummary Evaluation,
    bool WasSkipped)
{
    /// <summary>Erzeugt das Ergebnis eines wegen eines parallelen Laufs übersprungenen Checks.</summary>
    public static WatcherRunSummary Skipped(DateTimeOffset utcNow) =>
        new(
            utcNow,
            utcNow,
            0,
            new PriorityMailEvaluationSummary(0, 0, 0, 0, 0, 0),
            true);
}
