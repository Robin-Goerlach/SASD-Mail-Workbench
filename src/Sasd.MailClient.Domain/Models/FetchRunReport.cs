using System.Collections.Generic;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Kleine Zusammenfassung eines Abruf- und Verarbeitungslaufs.
/// Dies ist besonders hilfreich fuer spaetere GUI-Anzeigen oder Konsolenprotokolle.
/// </summary>
public sealed class FetchRunReport
{
    public int DiscoveredRemoteMessages { get; set; }

    public int NewlyImportedMessages { get; set; }

    public int SkippedAlreadyKnownMessages { get; set; }

    public List<string> ImportedMessageIds { get; set; } = new List<string>();

    public List<string> Warnings { get; set; } = new List<string>();

    public List<string> Errors { get; set; } = new List<string>();
}
