using System;
using System.Collections.Generic;
using Sasd.MailClient.Domain.Enums;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Repräsentiert die normalisierte, fachlich nutzbare Sicht auf eine Nachricht.
/// Diese Klasse ist bewusst etwas ausfuehrlicher, weil sie spaeter der zentrale Anker
/// fuer Filterung, Anzeige und automatische Weiterverarbeitung wird.
/// </summary>
public sealed class ParsedMessage
{
    public string Id { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    public string HeaderMessageId { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public MailAddress? From { get; set; }

    public List<MailAddress> To { get; set; } = new List<MailAddress>();

    public List<MailAddress> Cc { get; set; } = new List<MailAddress>();

    public MailAddress? ReplyTo { get; set; }

    public DateTimeOffset? SentAtUtc { get; set; }

    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string TextBody { get; set; } = string.Empty;

    public string HtmlBody { get; set; } = string.Empty;

    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public List<AttachmentInfo> Attachments { get; set; } = new List<AttachmentInfo>();

    public string NormalizedText { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new List<string>();

    public MessageProcessingState State { get; set; } = MessageProcessingState.Parsed;
}
