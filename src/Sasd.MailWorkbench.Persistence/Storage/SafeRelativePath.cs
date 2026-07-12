namespace Sasd.MailWorkbench.Persistence.Storage;

internal static class SafeRelativePath
{
    public static string Resolve(string rootDirectory, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("Es ist ein relativer Speicherpfad erforderlich.", nameof(relativePath));
        }

        string root = Path.GetFullPath(rootDirectory);
        string combined = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Der relative Pfad verlässt den konfigurierten Profilstamm.");
        }

        return combined;
    }

    public static string Normalize(string relativePath) =>
        relativePath.Replace('\\', '/');
}
