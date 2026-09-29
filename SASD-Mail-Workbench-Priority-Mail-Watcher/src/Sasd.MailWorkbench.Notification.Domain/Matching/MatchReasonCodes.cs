namespace Sasd.MailWorkbench.Notification.Domain.Matching;

/// <summary>Definiert stabile, protokollierbare Codes für positive Regelbedingungen.</summary>
public static class MatchReasonCodes
{
    /// <summary>Das Konto stimmt mit der Regel überein.</summary>
    public const string Account = "account";

    /// <summary>Eine exakte Absenderadresse stimmt überein.</summary>
    public const string SenderAddress = "sender-address";

    /// <summary>Eine Absenderdomain stimmt überein.</summary>
    public const string SenderDomain = "sender-domain";

    /// <summary>Ein Betreffbegriff wurde gefunden.</summary>
    public const string Subject = "subject";

    /// <summary>Ein Schlüsselwort wurde im freigegebenen Suchtext gefunden.</summary>
    public const string Keyword = "keyword";

    /// <summary>Die Anhangsbedingung stimmt überein.</summary>
    public const string Attachment = "attachment";
}
