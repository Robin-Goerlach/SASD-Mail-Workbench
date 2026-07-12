using System.IO;

namespace Sasd.MailClient.Bootstrap.ConsoleApp;

/// <summary>
/// Haelt die Pfade der lokalen Arbeitsstruktur an einer zentralen Stelle zusammen.
/// Das vermeidet String-Wildwuchs im Einstiegscode und macht den Wechsel von JSON auf
/// SQLite an genau einer Stelle sichtbar.
/// </summary>
public sealed class AppPathLayout
{
    public AppPathLayout(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        DatabaseDirectory = Path.Combine(rootDirectory, "database");
        DatabaseFilePath = Path.Combine(DatabaseDirectory, "mailclient.db");
        RawMailDirectory = Path.Combine(rootDirectory, "raw-mails");
        TempDirectory = Path.Combine(rootDirectory, "temp");
    }

    public string RootDirectory { get; }

    public string DatabaseDirectory { get; }

    /// <summary>
    /// Voller Pfad zur SQLite-Datenbankdatei.
    /// Diese Trennung ist hilfreich, weil manche Aufrufer nur das Verzeichnis kennen,
    /// waehrend die Repository-Implementierung die konkrete Dateiangabe braucht.
    /// </summary>
    public string DatabaseFilePath { get; }

    public string RawMailDirectory { get; }

    public string TempDirectory { get; }
}
