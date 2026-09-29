namespace Sasd.MailWorkbench.Notification.Contracts.Models;

/// <summary>
/// Identifiziert eine Benachrichtigung eindeutig anhand von Regel, Konto und technischer Mail-ID.
/// </summary>
public readonly record struct NotificationDispatchKey(
    Guid RuleId,
    string AccountId,
    string StableMessageId);
