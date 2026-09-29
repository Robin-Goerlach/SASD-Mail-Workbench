namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Fasst die fachliche Bewertung eines Beobachtungssnapshots zusammen.</summary>
public sealed record PriorityMailEvaluationSummary(
    int ObservedMessages,
    int EvaluatedRuleCombinations,
    int MatchingRuleCombinations,
    int PublishedNotifications,
    int SuppressedDuplicates,
    int FailedNotifications);
