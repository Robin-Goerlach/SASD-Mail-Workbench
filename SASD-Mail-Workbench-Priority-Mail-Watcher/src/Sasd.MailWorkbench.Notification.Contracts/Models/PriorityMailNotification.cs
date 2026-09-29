using System.Collections.ObjectModel;

namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Enthält die für eine Benutzerbenachrichtigung erforderlichen neutralen Daten.</summary>
public sealed class PriorityMailNotification
{
    /// <summary>Initialisiert eine neue Benachrichtigung.</summary>
    public PriorityMailNotification(
        Guid ruleId,
        string ruleName,
        string accountId,
        string stableMessageId,
        string senderAddress,
        string subject,
        DateTimeOffset receivedAtUtc,
        IEnumerable<string> matchReasonCodes)
    {
        if (ruleId == Guid.Empty)
        {
            throw new ArgumentException("Die Regel-ID darf nicht leer sein.", nameof(ruleId));
        }

        RuleId = ruleId;
        RuleName = RequireValue(ruleName, nameof(ruleName));
        AccountId = RequireValue(accountId, nameof(accountId));
        StableMessageId = RequireValue(stableMessageId, nameof(stableMessageId));
        SenderAddress = RequireValue(senderAddress, nameof(senderAddress));
        Subject = subject?.Trim() ?? string.Empty;
        ReceivedAtUtc = receivedAtUtc;

        ArgumentNullException.ThrowIfNull(matchReasonCodes);
        MatchReasonCodes = new ReadOnlyCollection<string>(
            matchReasonCodes.Distinct(StringComparer.Ordinal).ToArray());
    }

    /// <summary>Ruft die auslösende Regel-ID ab.</summary>
    public Guid RuleId { get; }

    /// <summary>Ruft den Anzeigenamen der auslösenden Regel ab.</summary>
    public string RuleName { get; }

    /// <summary>Ruft die Konto-ID ab.</summary>
    public string AccountId { get; }

    /// <summary>Ruft die stabile technische Nachrichtenidentität ab.</summary>
    public string StableMessageId { get; }

    /// <summary>Ruft die normalisierte Absenderadresse ab.</summary>
    public string SenderAddress { get; }

    /// <summary>Ruft den Betreff ab.</summary>
    public string Subject { get; }

    /// <summary>Ruft den Empfangszeitpunkt ab.</summary>
    public DateTimeOffset ReceivedAtUtc { get; }

    /// <summary>Ruft die fachlichen Treffercodes ab.</summary>
    public IReadOnlyList<string> MatchReasonCodes { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Der Wert darf nicht leer sein.", parameterName);
        }

        return value.Trim();
    }
}
