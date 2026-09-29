namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>Beschreibt einen nichtveränderlichen Laufzeitstatus des Watchers.</summary>
public sealed record WatcherStatus(
    WatcherOperationalState State,
    DateTimeOffset ChangedAtUtc,
    string Message,
    string? DiagnosticCode = null);
