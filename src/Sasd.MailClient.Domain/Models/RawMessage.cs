using System;
using Sasd.MailClient.Domain.Enums;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Repräsentiert die lokal gespeicherte Rohsicht einer Nachricht.
/// Der eigentliche Inhalt liegt im Dateisystem. Dieses Objekt enthaelt die Metadaten,
/// die fuer Wiederfinden, Reprocessing und Diagnose benoetigt werden.
/// </summary>
public sealed class RawMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string AccountId { get; set; } = string.Empty;

    public string ExternalMessageKey { get; set; } = string.Empty;

    public string SourceIdentifier { get; set; } = string.Empty;

    public string Fingerprint { get; set; } = string.Empty;

    public DateTimeOffset RetrievedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string StoragePath { get; set; } = string.Empty;

    public long ApproximateSizeInBytes { get; set; }

    public MessageProcessingState State { get; set; } = MessageProcessingState.RawStored;
}
