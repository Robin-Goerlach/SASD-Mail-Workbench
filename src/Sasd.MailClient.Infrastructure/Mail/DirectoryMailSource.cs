using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Mail;

/// <summary>
/// Demo-Implementierung einer Mailquelle auf Basis lokaler .eml-Dateien.
/// Dies erlaubt einen stabilen und reproduzierbaren Projektstart ohne echte Netzwerk- und Providerabhaengigkeit.
/// </summary>
public sealed class DirectoryMailSource : IMailSource
{
    private readonly string _inboxDirectory;

    public DirectoryMailSource(string inboxDirectory)
    {
        _inboxDirectory = inboxDirectory;
    }

    public Task<MailSourceConnectionTestResult> TestConnectionAsync(
        MailAccount account,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool exists = Directory.Exists(_inboxDirectory);

        MailSourceConnectionTestResult result = new MailSourceConnectionTestResult
        {
            Success = exists,
            TechnicalMessage = exists
                ? $"Lokales Verzeichnis '{_inboxDirectory}' gefunden."
                : $"Lokales Verzeichnis '{_inboxDirectory}' wurde nicht gefunden."
        };

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<RemoteMessageInfo>> ListAvailableMessagesAsync(
        MailAccount account,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_inboxDirectory))
        {
            return Task.FromResult<IReadOnlyList<RemoteMessageInfo>>(Array.Empty<RemoteMessageInfo>());
        }

        IReadOnlyList<RemoteMessageInfo> messages = Directory
            .EnumerateFiles(_inboxDirectory, "*.eml", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                FileInfo fileInfo = new FileInfo(path);

                return new RemoteMessageInfo
                {
                    ExternalKey = Path.GetFileName(path),
                    SourceIdentifier = path,
                    ApproximateSizeInBytes = fileInfo.Length
                };
            })
            .ToList();

        return Task.FromResult(messages);
    }

    public async Task<RemoteMessageContent> FetchMessageAsync(
        MailAccount account,
        RemoteMessageInfo remoteMessage,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string rawContent = await File.ReadAllTextAsync(
            remoteMessage.SourceIdentifier,
            cancellationToken);

        return new RemoteMessageContent
        {
            RemoteMessage = remoteMessage,
            RawContent = rawContent
        };
    }
}
