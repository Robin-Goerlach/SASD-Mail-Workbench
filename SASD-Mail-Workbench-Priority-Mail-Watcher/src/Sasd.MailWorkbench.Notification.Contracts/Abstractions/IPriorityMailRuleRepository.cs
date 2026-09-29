using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Contracts.Abstractions;

/// <summary>Stellt die zum Prüfzeitpunkt aktiven Beobachtungsregeln bereit.</summary>
public interface IPriorityMailRuleRepository
{
    /// <summary>Liest einen unveränderlichen Snapshot der aktiven Regeln.</summary>
    Task<IReadOnlyList<PriorityMailRule>> GetActiveRulesAsync(
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default);
}
