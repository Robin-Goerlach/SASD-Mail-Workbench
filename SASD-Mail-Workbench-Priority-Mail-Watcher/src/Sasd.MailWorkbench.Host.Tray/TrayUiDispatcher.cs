using System.Windows.Forms;

namespace Sasd.MailWorkbench.Host.Tray;

/// <summary>
/// Kapselt das Marshalling von Hintergrundcallbacks auf den WinForms-UI-Thread.
/// </summary>
internal sealed class TrayUiDispatcher : Control
{
    public TrayUiDispatcher()
    {
        // ApplicationContext besitzt kein eigenes Fenster. Das explizite Handle stellt deshalb
        // einen stabilen Dispatcher für Status- und Benachrichtigungscallbacks bereit.
        _ = Handle;
    }

    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }
}
