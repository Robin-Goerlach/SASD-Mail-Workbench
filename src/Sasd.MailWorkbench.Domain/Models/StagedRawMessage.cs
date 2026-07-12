using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Domain.Models;

/// <summary>
/// Ergebnis einer vollständig geschriebenen temporären Rohnachricht.
/// </summary>
/// <param name="StagingRelativePath">Relativer Pfad der temporären Datei.</param>
/// <param name="Fingerprint">Fingerprint der tatsächlich geschriebenen Bytes.</param>
public sealed record StagedRawMessage(
    string StagingRelativePath,
    MessageFingerprint Fingerprint);
