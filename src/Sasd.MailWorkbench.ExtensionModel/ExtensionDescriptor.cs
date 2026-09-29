namespace Sasd.MailWorkbench.ExtensionModel;

/// <summary>
/// Stabiler, UI- und Infrastruktur-unabhängiger Vertrag für spätere interne
/// Processor- und Analyzer-Erweiterungen.
/// </summary>
public sealed record ExtensionDescriptor
{
    public ExtensionDescriptor(
        string id,
        Version implementationVersion,
        int resultSchemaVersion)
    {
        if (string.IsNullOrWhiteSpace(id) || !id.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException("Die Extension-ID muss stabil und namespaced sein.", nameof(id));
        }

        if (resultSchemaVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(resultSchemaVersion));
        }

        Id = id;
        ImplementationVersion = implementationVersion ?? throw new ArgumentNullException(nameof(implementationVersion));
        ResultSchemaVersion = resultSchemaVersion;
    }

    public string Id { get; }

    public Version ImplementationVersion { get; }

    public int ResultSchemaVersion { get; }
}
