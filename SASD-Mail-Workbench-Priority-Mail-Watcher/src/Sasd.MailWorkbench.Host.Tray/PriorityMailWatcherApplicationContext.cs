using System.Drawing;
using System.Windows.Forms;
using Sasd.MailWorkbench.Notification.Application.Services;
using Sasd.MailWorkbench.Notification.Contracts.Models;

namespace Sasd.MailWorkbench.Host.Tray;

/// <summary>
/// Steuert Lebenszyklus, Tray-Menü und periodische Hintergrundausführung des Watchers.
/// </summary>
internal sealed class PriorityMailWatcherApplicationContext : ApplicationContext
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(2);

    private readonly CancellationTokenSource _shutdown = new();
    private readonly TrayUiDispatcher _dispatcher = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly PriorityMailWatcherCoordinator _coordinator;

    private bool _isEnabled;
    private Task _currentRun = Task.CompletedTask;
    private int _runInProgress;
    private bool _disposed;

    public PriorityMailWatcherApplicationContext()
    {
        _statusItem = new ToolStripMenuItem("Status: pausiert") { Enabled = false };
        _toggleItem = new ToolStripMenuItem("Überwachung starten", null, ToggleMonitoring);
        var checkNowItem = new ToolStripMenuItem("Jetzt prüfen", null, CheckNow);
        var exitItem = new ToolStripMenuItem("Beenden", null, ExitApplication);

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.AddRange(
            new ToolStripItem[]
            {
                _statusItem,
                new ToolStripSeparator(),
                _toggleItem,
                checkNowItem,
                new ToolStripSeparator(),
                exitItem
            });

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = contextMenu,
            Icon = SystemIcons.Information,
            Text = "SASD Priority Mail Watcher – pausiert",
            Visible = true
        };

        _coordinator = WatcherCompositionRoot.Create(_notifyIcon, _dispatcher, ApplyStatus);

        _timer = new System.Windows.Forms.Timer
        {
            Interval = checked((int)PollInterval.TotalMilliseconds),
            Enabled = false
        };
        _timer.Tick += TimerTick;
    }

    private void ToggleMonitoring(object? sender, EventArgs e)
    {
        _isEnabled = !_isEnabled;
        _timer.Enabled = _isEnabled;
        _toggleItem.Text = _isEnabled ? "Überwachung pausieren" : "Überwachung starten";

        if (_isEnabled)
        {
            StartRun();
        }
        else
        {
            ApplyStatus(new WatcherStatus(WatcherOperationalState.Paused, DateTimeOffset.UtcNow, "Überwachung wurde vom Benutzer pausiert."));
        }
    }

    private void CheckNow(object? sender, EventArgs e) => StartRun();

    private void TimerTick(object? sender, EventArgs e) => StartRun();

    private void StartRun()
    {
        if (Interlocked.CompareExchange(ref _runInProgress, 1, 0) != 0)
        {
            return;
        }

        _currentRun = RunOnceCoreAsync();
    }

    private async Task RunOnceCoreAsync()
    {
        try
        {
            await _coordinator.RunOnceAsync(_shutdown.Token).ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
        {
            // Normales Beenden des Tray-Prozesses.
        }
        catch (Exception exception)
        {
            ApplyStatus(new WatcherStatus(
                WatcherOperationalState.Faulted,
                DateTimeOffset.UtcNow,
                "Der Prüflauf ist unerwartet fehlgeschlagen.",
                exception.GetType().Name));
        }
        finally
        {
            Interlocked.Exchange(ref _runInProgress, 0);
        }
    }

    private void ApplyStatus(WatcherStatus status)
    {
        var text = status.State switch
        {
            WatcherOperationalState.Checking => "Status: Prüfung läuft",
            WatcherOperationalState.Idle => "Status: bereit",
            WatcherOperationalState.Paused => "Status: pausiert",
            WatcherOperationalState.Faulted => "Status: Fehler",
            WatcherOperationalState.Stopped => "Status: wird beendet",
            _ => "Status: unbekannt"
        };

        _statusItem.Text = text;
        _notifyIcon.Text = TruncateToolTip($"SASD Priority Mail Watcher – {text[8..]}");
    }

    private static string TruncateToolTip(string value) =>
        value.Length <= 63 ? value : value[..63];

    private async void ExitApplication(object? sender, EventArgs e)
    {
        _timer.Stop();
        _toggleItem.Enabled = false;
        _shutdown.Cancel();
        ApplyStatus(new WatcherStatus(WatcherOperationalState.Stopped, DateTimeOffset.UtcNow, "Watcher wird beendet."));

        try
        {
            await _currentRun.ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
        {
            // Der aktive Lauf wurde für das reguläre Beenden abgebrochen.
        }
        finally
        {
            ExitThread();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _timer.Stop();
            _timer.Tick -= TimerTick;
            _timer.Dispose();
            _shutdown.Cancel();
            _shutdown.Dispose();
            _coordinator.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.Dispose();
            _dispatcher.Dispose();
        }

        base.Dispose(disposing);
    }
}
