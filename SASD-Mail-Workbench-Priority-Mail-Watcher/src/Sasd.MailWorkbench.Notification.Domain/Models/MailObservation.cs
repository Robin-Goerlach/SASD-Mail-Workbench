namespace Sasd.MailWorkbench.Notification.Domain.Models;

/// <summary>
/// Beschreibt die für eine Beobachtungsregel erforderlichen, normalisierten Metadaten einer Mail.
/// </summary>
/// <remarks>
/// Der Typ enthält bewusst weder MailKit- noch Protokolltypen. Der gemeinsame Mailzugriff der
/// Workbench muss IMAP-, POP3- oder lokale Nachrichtendaten vor Übergabe an den Watcher in
/// dieses neutrale Modell abbilden.
/// </remarks>
public sealed class MailObservation
{
    /// <summary>
    /// Initialisiert eine neue Mailbeobachtung.
    /// </summary>
    /// <param name="accountId">Stabile ID des Kontos, aus dem die Nachricht stammt.</param>
    /// <param name="stableMessageId">
    /// Pro Konto stabile technische Nachrichtenidentität. Bei IMAP sollte sie mindestens
    /// UIDVALIDITY, Ordneridentität und UID berücksichtigen; eine Message-ID allein genügt nicht.
    /// </param>
    /// <param name="fromAddress">Normalisierte Absenderadresse.</param>
    /// <param name="replyToAddress">Optionale normalisierte Reply-To-Adresse.</param>
    /// <param name="subject">Betreff der Nachricht.</param>
    /// <param name="searchText">Für Schlüsselwortregeln freigegebener, lokaler Suchtext.</param>
    /// <param name="hasAttachments">Gibt an, ob die Nachricht mindestens einen Anhang besitzt.</param>
    /// <param name="receivedAtUtc">Empfangszeitpunkt in UTC beziehungsweise mit Offset.</param>
    public MailObservation(
        string accountId,
        string stableMessageId,
        string fromAddress,
        string? replyToAddress,
        string? subject,
        string? searchText,
        bool hasAttachments,
        DateTimeOffset receivedAtUtc)
    {
        AccountId = RequireValue(accountId, nameof(accountId));
        StableMessageId = RequireValue(stableMessageId, nameof(stableMessageId));
        FromAddress = NormalizeAddress(fromAddress, nameof(fromAddress));
        ReplyToAddress = string.IsNullOrWhiteSpace(replyToAddress)
            ? null
            : NormalizeAddress(replyToAddress, nameof(replyToAddress));
        Subject = subject?.Trim() ?? string.Empty;
        SearchText = searchText?.Trim() ?? string.Empty;
        HasAttachments = hasAttachments;
        ReceivedAtUtc = receivedAtUtc;
    }

    /// <summary>Ruft die stabile Konto-ID ab.</summary>
    public string AccountId { get; }

    /// <summary>Ruft die stabile technische Nachrichtenidentität ab.</summary>
    public string StableMessageId { get; }

    /// <summary>Ruft die normalisierte Absenderadresse ab.</summary>
    public string FromAddress { get; }

    /// <summary>Ruft die optionale normalisierte Reply-To-Adresse ab.</summary>
    public string? ReplyToAddress { get; }

    /// <summary>Ruft den Betreff ab.</summary>
    public string Subject { get; }

    /// <summary>Ruft den für lokale Schlüsselwortsuche freigegebenen Text ab.</summary>
    public string SearchText { get; }

    /// <summary>Ruft ab, ob die Nachricht Anhänge besitzt.</summary>
    public bool HasAttachments { get; }

    /// <summary>Ruft den Empfangszeitpunkt ab.</summary>
    public DateTimeOffset ReceivedAtUtc { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Der Wert darf nicht leer sein.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeAddress(string value, string parameterName)
    {
        var normalized = RequireValue(value, parameterName).ToLowerInvariant();
        var separatorIndex = normalized.LastIndexOf('@');

        if (separatorIndex <= 0 || separatorIndex == normalized.Length - 1 || normalized.Contains(' '))
        {
            throw new ArgumentException("Die Adresse besitzt kein gültiges Grundformat.", parameterName);
        }

        return normalized;
    }
}
