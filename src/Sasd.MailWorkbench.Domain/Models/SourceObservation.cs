namespace Sasd.MailWorkbench.Domain.Models;

/// <summary>
/// Verknüpft einen providerseitigen Quellschlüssel mit der kanonischen lokalen Rohnachricht.
/// </summary>
/// <param name="AccountId">Logische Konto-ID.</param>
/// <param name="SourceKey">Quellseitiger Schlüssel, später beispielsweise eine POP3-UIDL.</param>
/// <param name="MessageId">ID der kanonischen lokalen Rohnachricht.</param>
/// <param name="FirstSeenAtUtc">Erstes beobachtetes Auftreten.</param>
/// <param name="LastSeenAtUtc">Letztes beobachtetes Auftreten.</param>
public sealed record SourceObservation(
    string AccountId,
    string SourceKey,
    string MessageId,
    DateTimeOffset FirstSeenAtUtc,
    DateTimeOffset LastSeenAtUtc);
