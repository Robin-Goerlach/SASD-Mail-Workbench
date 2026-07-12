using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Processing;

/// <summary>
/// Extrahiert einfache, gut erklaerbare Struktur aus dem Nachrichtentext.
/// Der Fokus liegt absichtlich auf robusten und nachvollziehbaren Regeln statt auf Magie.
/// </summary>
public sealed class KeyValueBodyExtractionProcessor : IMessageProcessor
{
    private static readonly string[] InterestingKeys =
    {
        "Ticket",
        "Project",
        "Projekt",
        "Invoice",
        "Rechnung",
        "Order",
        "Customer",
        "Kunde"
    };

    public string Name => nameof(KeyValueBodyExtractionProcessor);

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

        foreach (string key in InterestingKeys)
        {
            string? value = ExtractValue(message.NormalizedText, key);

            if (!string.IsNullOrWhiteSpace(value))
            {
                result.ExtractedData[key] = value!;
            }
        }

        if (result.ExtractedData.Count == 0)
        {
            result.Notes.Add(new ProcessingNote
            {
                Level = ProcessingNoteLevel.Information,
                Source = Name,
                Message = "Es wurden keine bekannten Key-Value-Muster im Body gefunden."
            });
        }

        result.FinishedAtUtc = DateTimeOffset.UtcNow;
        return Task.FromResult(result);
    }

    private static string? ExtractValue(string text, string key)
    {
        string pattern = $@"(?im)^\s*{Regex.Escape(key)}\s*:\s*(?<value>.+?)\s*$";
        Match match = Regex.Match(text, pattern);

        return match.Success
            ? match.Groups["value"].Value.Trim()
            : null;
    }
}
