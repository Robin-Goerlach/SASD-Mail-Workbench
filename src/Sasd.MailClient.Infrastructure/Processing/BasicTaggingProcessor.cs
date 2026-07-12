using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Processing;

/// <summary>
/// Vergibt einfache fachliche Tags anhand offensichtlicher Schluesselwoerter.
/// Dies ist bewusst kein intelligentes Klassifikationssystem, sondern ein transparenter
/// Startpunkt, der spaeter leicht ausgebaut oder ersetzt werden kann.
/// </summary>
public sealed class BasicTaggingProcessor : IMessageProcessor
{
    private static readonly Dictionary<string, string> KeywordToTagMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["invoice"] = "finance",
            ["rechnung"] = "finance",
            ["project"] = "project",
            ["projekt"] = "project",
            ["ticket"] = "support",
            ["support"] = "support",
            ["alert"] = "alert",
            ["warning"] = "alert",
            ["angebot"] = "sales",
            ["proposal"] = "sales"
        };

    public string Name => nameof(BasicTaggingProcessor);

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

        string searchText = $"{message.Subject}\n{message.NormalizedText}";

        foreach (KeyValuePair<string, string> entry in KeywordToTagMap)
        {
            bool alreadyTagged = message.Tags.Any(tag =>
                string.Equals(tag, entry.Value, StringComparison.OrdinalIgnoreCase));

            if (searchText.Contains(entry.Key, StringComparison.OrdinalIgnoreCase) && !alreadyTagged)
            {
                message.Tags.Add(entry.Value);
                result.Tags.Add(entry.Value);
            }
        }

        result.FinishedAtUtc = DateTimeOffset.UtcNow;
        return Task.FromResult(result);
    }
}
