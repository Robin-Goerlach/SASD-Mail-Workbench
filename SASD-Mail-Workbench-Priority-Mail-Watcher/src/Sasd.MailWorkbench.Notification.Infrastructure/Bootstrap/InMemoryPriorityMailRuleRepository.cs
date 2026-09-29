using System.Collections.ObjectModel;
using Sasd.MailWorkbench.Notification.Contracts.Abstractions;
using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Infrastructure.Bootstrap;

/// <summary>
/// Thread-sicherer In-Memory-Regelsnapshot für Bootstrap und frühe Integrationsprüfungen.
/// </summary>
/// <remarks>
/// Diese Implementierung ist nicht für produktive Persistenz vorgesehen. Der spätere
/// SQLite-Adapter muss dieselbe Schnittstelle implementieren.
/// </remarks>
public sealed class InMemoryPriorityMailRuleRepository : IPriorityMailRuleRepository
{
    private readonly object _sync = new();
    private IReadOnlyList<PriorityMailRule> _rules;

    /// <summary>Initialisiert das Repository mit einem unveränderlichen Regelsnapshot.</summary>
    public InMemoryPriorityMailRuleRepository(IEnumerable<PriorityMailRule>? rules = null)
    {
        _rules = CreateSnapshot(rules ?? Array.Empty<PriorityMailRule>());
    }

    /// <summary>Ersetzt den vollständigen Regelsnapshot atomar.</summary>
    public void ReplaceAll(IEnumerable<PriorityMailRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var snapshot = CreateSnapshot(rules);

        lock (_sync)
        {
            _rules = snapshot;
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PriorityMailRule>> GetActiveRulesAsync(
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            var activeRules = _rules.Where(rule => rule.IsActiveAt(utcNow)).ToArray();
            return Task.FromResult<IReadOnlyList<PriorityMailRule>>(
                new ReadOnlyCollection<PriorityMailRule>(activeRules));
        }
    }

    private static IReadOnlyList<PriorityMailRule> CreateSnapshot(IEnumerable<PriorityMailRule> rules) =>
        new ReadOnlyCollection<PriorityMailRule>(rules.ToArray());
}
