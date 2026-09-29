using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Domain.Models;

/// <summary>
/// Beschreibt eine kanonische, dauerhaft gespeicherte Rohnachricht.
/// </summary>
/// <param name="Id">Eindeutige lokale Nachrichten-ID.</param>
/// <param name="AccountId">Logische Konto-ID.</param>
/// <param name="Fingerprint">Bytegenauer Fingerprint der Originaldaten.</param>
/// <param name="RelativePath">Relativer Pfad innerhalb des Profilstamms.</param>
/// <param name="ImportedAtUtc">UTC-Zeitpunkt der dauerhaften Übernahme.</param>
public sealed record StoredRawMessage(
    string Id,
    string AccountId,
    MessageFingerprint Fingerprint,
    string RelativePath,
    DateTimeOffset ImportedAtUtc);
