namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Definiert den aktuellen operativen Zustand des Hintergrundagenten.</summary>
public enum WatcherOperationalState
{
    /// <summary>Der Watcher ist angehalten und führt keine automatischen Prüfungen aus.</summary>
    Paused = 0,

    /// <summary>Der Watcher ist bereit und wartet auf die nächste Prüfung.</summary>
    Idle = 1,

    /// <summary>Der Watcher führt gerade eine Prüfung aus.</summary>
    Checking = 2,

    /// <summary>Der Watcher hat bei der letzten Prüfung einen Fehler festgestellt.</summary>
    Faulted = 3,

    /// <summary>Der Prozess wird beendet oder wurde beendet.</summary>
    Stopped = 4
}
