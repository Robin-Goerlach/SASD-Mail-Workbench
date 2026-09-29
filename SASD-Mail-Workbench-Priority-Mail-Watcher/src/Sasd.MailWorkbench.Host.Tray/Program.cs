using System.Windows.Forms;

namespace Sasd.MailWorkbench.Host.Tray;

internal static class Program
{
    /// <summary>Startet den benutzergebundenen Tray-Prozess.</summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PriorityMailWatcherApplicationContext());
    }
}
