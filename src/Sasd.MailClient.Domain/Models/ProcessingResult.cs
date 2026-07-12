using System;
using System.Collections.Generic;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Ergebnis eines einzelnen Verarbeitungsschrittes.
/// Mehrere dieser Ergebnisse koennen spaeter zu einem Gesamtbild pro Nachricht kombiniert werden.
/// </summary>
public sealed class ProcessingResult
{
    public string MessageId { get; set; } = string.Empty;

    public string ProcessorName { get; set; } = string.Empty;

    public bool Success { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset FinishedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<string> Tags { get; set; } = new List<string>();

    public Dictionary<string, string> ExtractedData { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public List<ProcessingNote> Notes { get; set; } = new List<ProcessingNote>();
}
