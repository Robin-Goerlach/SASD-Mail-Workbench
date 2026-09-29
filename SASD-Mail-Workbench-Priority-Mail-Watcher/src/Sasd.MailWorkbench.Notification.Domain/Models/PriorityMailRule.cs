namespace Sasd.MailWorkbench.Notification.Domain.Models;

/// <summary>
/// Repräsentiert eine fachliche Beobachtungsregel für relevante E-Mails.
/// </summary>
public sealed class PriorityMailRule
{
    /// <summary>Initialisiert eine neue Beobachtungsregel.</summary>
    public PriorityMailRule(
        Guid id,
        string name,
        PriorityMailRuleCriteria criteria,
        string? accountId = null,
        bool isEnabled = true,
        DateTimeOffset? activeFromUtc = null,
        DateTimeOffset? expiresAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Die Regel-ID darf nicht leer sein.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Der Regelname darf nicht leer sein.", nameof(name));
        }

        if (activeFromUtc.HasValue && expiresAtUtc.HasValue && expiresAtUtc <= activeFromUtc)
        {
            throw new ArgumentException(
                "Das Ablaufdatum muss nach dem optionalen Aktivierungsdatum liegen.",
                nameof(expiresAtUtc));
        }

        Id = id;
        Name = name.Trim();
        Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        AccountId = string.IsNullOrWhiteSpace(accountId) ? null : accountId.Trim();
        IsEnabled = isEnabled;
        ActiveFromUtc = activeFromUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    /// <summary>Ruft die stabile Regel-ID ab.</summary>
    public Guid Id { get; }

    /// <summary>Ruft den Anzeigenamen der Regel ab.</summary>
    public string Name { get; }

    /// <summary>Ruft die Vergleichskriterien ab.</summary>
    public PriorityMailRuleCriteria Criteria { get; }

    /// <summary>Ruft die optionale Einschränkung auf ein bestimmtes Konto ab.</summary>
    public string? AccountId { get; }

    /// <summary>Ruft ab, ob die Regel administrativ aktiviert ist.</summary>
    public bool IsEnabled { get; }

    /// <summary>Ruft den optionalen Beginn des Gültigkeitsfensters ab.</summary>
    public DateTimeOffset? ActiveFromUtc { get; }

    /// <summary>Ruft den optionalen exklusiven Ablaufzeitpunkt ab.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; }

    /// <summary>Prüft, ob die Regel zum angegebenen Zeitpunkt aktiv ist.</summary>
    public bool IsActiveAt(DateTimeOffset utcNow)
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (ActiveFromUtc.HasValue && utcNow < ActiveFromUtc.Value)
        {
            return false;
        }

        return !ExpiresAtUtc.HasValue || utcNow < ExpiresAtUtc.Value;
    }
}
