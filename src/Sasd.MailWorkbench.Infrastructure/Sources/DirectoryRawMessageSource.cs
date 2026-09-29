using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Domain.Models;

namespace Sasd.MailWorkbench.Infrastructure.Sources;

/// <summary>
/// Lokale EML-Verzeichnisquelle für Demo-, Integrations- und Recoverytests.
/// </summary>
/// <remarks>
/// Relative Dateinamen simulieren in Milestone 0.3.1 die späteren POP3-UIDLs.
/// Die Klasse ist kein Produktiv-Ersatz für POP3, sondern ein deterministischer
/// Adapter zum Testen des Importkerns ohne Netzwerk und Zugangsdaten.
/// </remarks>
public sealed class DirectoryRawMessageSource : IRawMessageSource
{
    private readonly string _directory;

    /// <summary>
    /// Erstellt eine Quelle für ein lokales Verzeichnis.
    /// </summary>
    public DirectoryRawMessageSource(string directory)
    {
        _directory = Path.GetFullPath(directory ?? throw new ArgumentNullException(nameof(directory)));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<RawMessageSourceItem>> ListAsync(
        string accountId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(_directory);

        IReadOnlyList<RawMessageSourceItem> items = Directory
            .EnumerateFiles(_directory, "*.eml", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new RawMessageSourceItem(Path.GetFileName(path), Path.GetFileName(path)))
            .ToArray();
        return Task.FromResult(items);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(
        string accountId,
        RawMessageSourceItem item,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Path.GetFileName verhindert, dass ein manipuliertes SourceKey-Feld
        // außerhalb des konfigurierten Demo-Verzeichnisses lesen kann.
        string fileName = Path.GetFileName(item.SourceKey);
        string path = Path.Combine(_directory, fileName);
        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }
}
