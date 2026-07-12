using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Persistence.Storage;

/// <summary>
/// Speichert Rohmails relativ zu einem Profilstamm. Staging und endgültiger
/// Speicher liegen absichtlich auf demselben Dateisystem, damit die Umbenennung
/// atomar erfolgen kann.
/// </summary>
public sealed class FileSystemRawMessageStore : IRawMessageStore
{
    private readonly string _rootDirectory;
    private readonly IMessageFingerprintService _fingerprintService;

    /// <summary>
    /// Erstellt den Dateispeicher und legt Staging- sowie Rohmail-Verzeichnis an.
    /// </summary>
    public FileSystemRawMessageStore(
        string rootDirectory,
        IMessageFingerprintService fingerprintService)
    {
        _rootDirectory = Path.GetFullPath(rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory)));
        _fingerprintService = fingerprintService ?? throw new ArgumentNullException(nameof(fingerprintService));
        Directory.CreateDirectory(_rootDirectory);
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "staging"));
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "raw-mails"));
    }

    /// <inheritdoc />
    public async Task<StagedRawMessage> StageAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        string relativePath = SafeRelativePath.Normalize($"staging/{Guid.NewGuid():N}.eml.part");
        string fullPath = SafeRelativePath.Resolve(_rootDirectory, relativePath);

        try
        {
            await using FileStream destination = new(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 128 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            Sasd.MailWorkbench.Domain.ValueObjects.MessageFingerprint fingerprint = await _fingerprintService
                .CopyAndComputeAsync(source, destination, cancellationToken)
                .ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            destination.Flush(flushToDisk: true);
            return new StagedRawMessage(relativePath, fingerprint);
        }
        catch
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            throw;
        }
    }

    /// <inheritdoc />
    public string GetTargetRelativePath(string messageId, DateTimeOffset importedAtUtc)
    {
        string safeId = new string(messageId.Where(character => char.IsLetterOrDigit(character) || character is '-' or '_').ToArray());
        if (safeId.Length == 0)
        {
            throw new ArgumentException("Die Nachrichten-ID enthält keine zulässigen Zeichen.", nameof(messageId));
        }

        return SafeRelativePath.Normalize(
            $"raw-mails/{importedAtUtc:yyyy}/{importedAtUtc:MM}/{safeId}.eml");
    }

    /// <inheritdoc />
    public Task CommitAsync(
        string stagingRelativePath,
        string targetRelativePath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string source = SafeRelativePath.Resolve(_rootDirectory, stagingRelativePath);
        string target = SafeRelativePath.Resolve(_rootDirectory, targetRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Move(source, target, overwrite: false);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DiscardAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string fullPath = SafeRelativePath.Resolve(_rootDirectory, relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(SafeRelativePath.Resolve(_rootDirectory, relativePath)));
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            SafeRelativePath.Resolve(_rootDirectory, relativePath),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListStagingFilesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string staging = Path.Combine(_rootDirectory, "staging");
        IReadOnlyList<string> files = Directory
            .EnumerateFiles(staging, "*.part", SearchOption.TopDirectoryOnly)
            .Select(path => SafeRelativePath.Normalize(Path.GetRelativePath(_rootDirectory, path)))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        return Task.FromResult(files);
    }
}
