using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Verwaltet temporäre und kanonische Rohmail-Dateien relativ zu einem Profilstamm.
/// </summary>
public interface IRawMessageStore
{
    /// <summary>
    /// Schreibt eine Nachricht vollständig in den Stagingbereich und berechnet dabei den Fingerprint.
    /// </summary>
    Task<StagedRawMessage> StageAsync(Stream source, CancellationToken cancellationToken);

    /// <summary>
    /// Erzeugt einen stabilen relativen Zielpfad für eine kanonische Nachricht.
    /// </summary>
    string GetTargetRelativePath(string messageId, DateTimeOffset importedAtUtc);

    /// <summary>
    /// Verschiebt eine vollständig geschriebene Stagingdatei atomar an ihren Zielort.
    /// </summary>
    Task CommitAsync(
        string stagingRelativePath,
        string targetRelativePath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Entfernt eine nicht mehr benötigte lokale Datei idempotent.
    /// </summary>
    Task DiscardAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Prüft, ob ein relativer Speicherpfad existiert.
    /// </summary>
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Öffnet eine gespeicherte Datei zum Lesen.
    /// </summary>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Listet alle noch vorhandenen temporären Stagingdateien.
    /// </summary>
    Task<IReadOnlyList<string>> ListStagingFilesAsync(CancellationToken cancellationToken);
}
