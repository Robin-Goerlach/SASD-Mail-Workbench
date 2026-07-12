using System;
using System.IO;

namespace Sasd.MailClient.Tests.TestSupport;

/// <summary>
/// Verwaltet ein temporaeres Arbeitsverzeichnis fuer einen Testfall.
/// Durch die Kapselung wird das Aufraeumen am Testende einfacher und zuverlaessiger.
/// </summary>
public sealed class TestWorkspace : IDisposable
{
    public TestWorkspace(string testName)
    {
        string safeName = testName.Replace(' ', '-').Replace(':', '-').Replace('/', '-');
        RootDirectory = Path.Combine(Path.GetTempPath(), "Sasd.MailClient.Tests", safeName, Guid.NewGuid().ToString("N"));

        InboxDirectory = Path.Combine(RootDirectory, "samples", "inbox");
        DatabaseDirectory = Path.Combine(RootDirectory, "database");
        DatabaseFilePath = Path.Combine(DatabaseDirectory, "mailclient-tests.db");
        RawMailDirectory = Path.Combine(RootDirectory, "raw-mails");

        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(InboxDirectory);
        Directory.CreateDirectory(DatabaseDirectory);
        Directory.CreateDirectory(RawMailDirectory);
    }

    public string RootDirectory { get; }

    public string InboxDirectory { get; }

    public string DatabaseDirectory { get; }

    /// <summary>
    /// Voller Pfad zur SQLite-Datei des Testlaufs.
    /// Dadurch muessen einzelne Tests die Dateibenennung nicht selbst duplizieren.
    /// </summary>
    public string DatabaseFilePath { get; }

    public string RawMailDirectory { get; }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RootDirectory))
            {
                Directory.Delete(RootDirectory, true);
            }
        }
        catch
        {
            // In Tests wollen wir Aufraeumprobleme nicht ueber das eigentliche Testergebnis stellen.
            // Falls ein Verzeichnis noch gesperrt ist, bleibt es ausnahmsweise liegen.
        }
    }
}
