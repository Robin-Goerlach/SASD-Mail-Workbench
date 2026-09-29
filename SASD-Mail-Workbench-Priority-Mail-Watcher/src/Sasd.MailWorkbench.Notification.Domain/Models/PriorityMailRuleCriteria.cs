using System.Collections.ObjectModel;

namespace Sasd.MailWorkbench.Notification.Domain.Models;

/// <summary>
/// Enthält die unveränderlichen Vergleichskriterien einer Priority-Mail-Regel.
/// </summary>
public sealed class PriorityMailRuleCriteria
{
    /// <summary>
    /// Initialisiert neue Regelkriterien.
    /// </summary>
    /// <remarks>
    /// Werte innerhalb einer Kategorie werden mit ODER verknüpft. Unterschiedliche Kategorien
    /// werden durch den Matcher mit UND verknüpft. Absenderadressen und Absenderdomains bilden
    /// gemeinsam eine Absenderkategorie.
    /// </remarks>
    public PriorityMailRuleCriteria(
        IEnumerable<string>? senderAddresses = null,
        IEnumerable<string>? senderDomains = null,
        IEnumerable<string>? subjectTerms = null,
        IEnumerable<string>? keywords = null,
        bool? requiresAttachments = null)
    {
        SenderAddresses = NormalizeDistinct(senderAddresses, NormalizeAddress);
        SenderDomains = NormalizeDistinct(senderDomains, NormalizeDomain);
        SubjectTerms = NormalizeDistinct(subjectTerms, NormalizeSearchTerm);
        Keywords = NormalizeDistinct(keywords, NormalizeSearchTerm);
        RequiresAttachments = requiresAttachments;

        if (SenderAddresses.Count == 0 &&
            SenderDomains.Count == 0 &&
            SubjectTerms.Count == 0 &&
            Keywords.Count == 0 &&
            RequiresAttachments is null)
        {
            throw new ArgumentException(
                "Eine Beobachtungsregel benötigt mindestens ein fachliches Vergleichskriterium.",
                nameof(senderAddresses));
        }
    }

    /// <summary>Ruft die beobachteten exakten Absenderadressen ab.</summary>
    public IReadOnlyList<string> SenderAddresses { get; }

    /// <summary>Ruft die beobachteten Absenderdomains ab.</summary>
    public IReadOnlyList<string> SenderDomains { get; }

    /// <summary>Ruft die Begriffe ab, von denen mindestens einer im Betreff vorkommen muss.</summary>
    public IReadOnlyList<string> SubjectTerms { get; }

    /// <summary>Ruft die Begriffe ab, von denen mindestens einer im freigegebenen Suchtext vorkommen muss.</summary>
    public IReadOnlyList<string> Keywords { get; }

    /// <summary>
    /// Ruft die optionale Anhangsbedingung ab. <see langword="null"/> bedeutet, dass Anhänge
    /// für die Regel keine Rolle spielen.
    /// </summary>
    public bool? RequiresAttachments { get; }

    private static IReadOnlyList<string> NormalizeDistinct(
        IEnumerable<string>? values,
        Func<string, string> normalizer)
    {
        if (values is null)
        {
            return Array.Empty<string>();
        }

        var normalizedValues = values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(normalizer)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ReadOnlyCollection<string>(normalizedValues);
    }

    private static string NormalizeAddress(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var separatorIndex = normalized.LastIndexOf('@');

        if (separatorIndex <= 0 || separatorIndex == normalized.Length - 1 || normalized.Contains(' '))
        {
            throw new ArgumentException($"Ungültige Absenderadresse: '{value}'.", nameof(value));
        }

        return normalized;
    }

    private static string NormalizeDomain(string value)
    {
        var normalized = value.Trim().TrimStart('@').TrimEnd('.').ToLowerInvariant();
        if (normalized.Length == 0 || normalized.Contains('@') || normalized.Contains(' '))
        {
            throw new ArgumentException($"Ungültige Absenderdomain: '{value}'.", nameof(value));
        }

        return normalized;
    }

    private static string NormalizeSearchTerm(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new ArgumentException("Ein Suchbegriff darf nicht leer sein.", nameof(value));
        }

        return normalized;
    }
}
