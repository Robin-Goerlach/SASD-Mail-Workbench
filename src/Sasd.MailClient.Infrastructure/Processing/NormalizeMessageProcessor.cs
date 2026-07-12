using System;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Processing;

/// <summary>
/// Stellt sicher, dass spaetere Prozessoren auf einem vernuenftig bereinigten Text arbeiten.
/// </summary>
public sealed class NormalizeMessageProcessor : IMessageProcessor
{
    public string Name => nameof(NormalizeMessageProcessor);

    public Task<ProcessingResult> ProcessAsync(
        ParsedMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProcessingResult result = new ProcessingResult
        {
            MessageId = message.Id,
            ProcessorName = Name,
            Success = true,
            StartedAtUtc = DateTimeOffset.UtcNow
        };

        if (string.IsNullOrWhiteSpace(message.NormalizedText))
        {
            result.Success = false;
            result.Notes.Add(new ProcessingNote
            {
                Level = ProcessingNoteLevel.Warning,
                Source = Name,
                Message = "Die Nachricht enthaelt keinen verwertbaren normalisierten Text."
            });
        }
        else
        {
            result.Notes.Add(new ProcessingNote
            {
                Level = ProcessingNoteLevel.Information,
                Source = Name,
                Message = "Normalisierter Nachrichtentext steht fuer Folgeprozessoren bereit."
            });
        }

        result.FinishedAtUtc = DateTimeOffset.UtcNow;
        return Task.FromResult(result);
    }
}
