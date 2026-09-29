using Sasd.MailWorkbench.Notification.Domain.Models;

namespace Sasd.MailWorkbench.Notification.Domain.Matching;

/// <summary>
/// Bewertet normalisierte Mailbeobachtungen gegen unveränderliche Priority-Mail-Regeln.
/// </summary>
public sealed class PriorityMailRuleMatcher
{
    /// <summary>Prüft, ob eine Beobachtung die angegebene Regel erfüllt.</summary>
    public PriorityMailMatchResult Match(
        PriorityMailRule rule,
        MailObservation observation,
        DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(observation);

        if (!rule.IsActiveAt(utcNow))
        {
            return PriorityMailMatchResult.NotMatched();
        }

        var reasons = new List<string>();

        if (rule.AccountId is not null)
        {
            if (!string.Equals(rule.AccountId, observation.AccountId, StringComparison.OrdinalIgnoreCase))
            {
                return PriorityMailMatchResult.NotMatched();
            }

            reasons.Add(MatchReasonCodes.Account);
        }

        if (!MatchesSender(rule.Criteria, observation.FromAddress, reasons))
        {
            return PriorityMailMatchResult.NotMatched();
        }

        if (rule.Criteria.SubjectTerms.Count > 0)
        {
            if (!ContainsAny(observation.Subject, rule.Criteria.SubjectTerms))
            {
                return PriorityMailMatchResult.NotMatched();
            }

            reasons.Add(MatchReasonCodes.Subject);
        }

        if (rule.Criteria.Keywords.Count > 0)
        {
            var searchableText = string.Concat(observation.Subject, "\n", observation.SearchText);
            if (!ContainsAny(searchableText, rule.Criteria.Keywords))
            {
                return PriorityMailMatchResult.NotMatched();
            }

            reasons.Add(MatchReasonCodes.Keyword);
        }

        if (rule.Criteria.RequiresAttachments.HasValue)
        {
            if (observation.HasAttachments != rule.Criteria.RequiresAttachments.Value)
            {
                return PriorityMailMatchResult.NotMatched();
            }

            reasons.Add(MatchReasonCodes.Attachment);
        }

        return PriorityMailMatchResult.Matched(reasons);
    }

    private static bool MatchesSender(
        PriorityMailRuleCriteria criteria,
        string fromAddress,
        ICollection<string> reasons)
    {
        if (criteria.SenderAddresses.Count == 0 && criteria.SenderDomains.Count == 0)
        {
            return true;
        }

        if (criteria.SenderAddresses.Contains(fromAddress, StringComparer.OrdinalIgnoreCase))
        {
            reasons.Add(MatchReasonCodes.SenderAddress);
            return true;
        }

        var domain = GetDomain(fromAddress);
        if (domain is not null && criteria.SenderDomains.Contains(domain, StringComparer.OrdinalIgnoreCase))
        {
            reasons.Add(MatchReasonCodes.SenderDomain);
            return true;
        }

        return false;
    }

    private static string? GetDomain(string address)
    {
        var separatorIndex = address.LastIndexOf('@');
        return separatorIndex >= 0 && separatorIndex < address.Length - 1
            ? address[(separatorIndex + 1)..]
            : null;
    }

    private static bool ContainsAny(string source, IReadOnlyList<string> searchTerms) =>
        searchTerms.Any(term => source.Contains(term, StringComparison.OrdinalIgnoreCase));
}
