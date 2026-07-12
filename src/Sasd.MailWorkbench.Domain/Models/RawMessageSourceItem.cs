namespace Sasd.MailWorkbench.Domain.Models;

/// <summary>
/// Providerneutrale Referenz auf eine sichtbare Rohnachricht.
/// </summary>
public sealed record RawMessageSourceItem
{
    /// <summary>
    /// Erstellt eine validierte Quellenreferenz.
    /// </summary>
    public RawMessageSourceItem(string sourceKey, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("Der Quellschlüssel darf nicht leer sein.", nameof(sourceKey));
        }

        SourceKey = sourceKey;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? sourceKey : displayName;
    }

    /// <summary>
    /// Stabiler Schlüssel der Quelle, später beispielsweise eine POP3-UIDL.
    /// </summary>
    public string SourceKey { get; }

    /// <summary>
    /// Diagnosefreundliche Bezeichnung der Quelle.
    /// </summary>
    public string DisplayName { get; }
}
