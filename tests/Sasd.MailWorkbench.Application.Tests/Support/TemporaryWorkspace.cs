namespace Sasd.MailWorkbench.Tests.Support;

internal sealed class TemporaryWorkspace : IDisposable
{
    public TemporaryWorkspace()
    {
        Root = Path.Combine(Path.GetTempPath(), "Sasd.MailWorkbench.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string PathFor(params string[] parts)
    {
        string path = parts.Aggregate(Root, Path.Combine);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? path);
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
        catch
        {
            // Aufräumfehler dürfen den eigentlichen Testbefund nicht verdecken.
        }
    }
}
