namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>
/// Enthält eine strukturierte technische Fehlerdiagnose ohne Nachrichtentext oder Mailbody.
/// </summary>
public sealed record WatcherFailure(
    DateTimeOffset OccurredAtUtc,
    string Stage,
    string FailureCode,
    string ExceptionType,
    Guid? RuleId = null,
    string? StableMessageId = null);
