using System.Collections.ObjectModel;

namespace Sasd.MailWorkbench.Notification.Domain.Matching;

/// <summary>Beschreibt das Ergebnis einer fachlichen Regelbewertung.</summary>
public sealed class PriorityMailMatchResult
{
    private PriorityMailMatchResult(bool isMatch, IReadOnlyList<string> reasonCodes)
    {
        IsMatch = isMatch;
        ReasonCodes = reasonCodes;
    }

    /// <summary>Ruft ab, ob die Beobachtung die Regel erfüllt.</summary>
    public bool IsMatch { get; }

    /// <summary>Ruft die stabilen Codes der erfüllten positiven Bedingungen ab.</summary>
    public IReadOnlyList<string> ReasonCodes { get; }

    /// <summary>Erzeugt ein positives Ergebnis.</summary>
    public static PriorityMailMatchResult Matched(IEnumerable<string> reasonCodes)
    {
        ArgumentNullException.ThrowIfNull(reasonCodes);
        var snapshot = reasonCodes.Distinct(StringComparer.Ordinal).ToArray();
        return new PriorityMailMatchResult(true, new ReadOnlyCollection<string>(snapshot));
    }

    /// <summary>Erzeugt ein negatives Ergebnis ohne fachliche Treffergründe.</summary>
    public static PriorityMailMatchResult NotMatched() =>
        new(false, Array.Empty<string>());
}
