using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Storage;

/// <summary>
/// Speichert Rohmails als einzelne Dateien im Dateisystem.
/// Dieses Modell ist fuer Reprocessing und Diagnose sehr angenehm, weil man Originalinhalte
/// leicht wiederfinden und manuell inspizieren kann.
/// </summary>
public sealed class FileSystemRawMessageStore : IRawMessageStore
{
    private readonly string _rootDirectory;

    public FileSystemRawMessageStore(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public async Task<string> SaveAsync(
        RawMessage message,
        string rawContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string yearDirectory = Path.Combine(_rootDirectory, DateTime.UtcNow.ToString("yyyy"));
        Directory.CreateDirectory(yearDirectory);

        string filePath = Path.Combine(yearDirectory, $"{message.Id}.eml");

        await File.WriteAllTextAsync(filePath, rawContent, cancellationToken);

        return filePath;
    }

    public Task<string> LoadAsync(
        RawMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return File.ReadAllTextAsync(message.StoragePath, cancellationToken);
    }
}
