using System;
using Sasd.MailClient.Domain.Enums;

namespace Sasd.MailClient.Domain.Models;

/// <summary>
/// Beschreibt ein Postfachprofil aus Sicht der Anwendung.
/// Zugangsdaten werden bewusst nicht hier gespeichert, damit dieses Objekt keine
/// Klartext-Secrets in sich traegt.
/// </summary>
public sealed class MailAccount
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string DisplayName { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public MailSecurityMode SecurityMode { get; set; } = MailSecurityMode.SslTls;

    public string UserName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastSuccessfulFetchUtc { get; set; }
}
