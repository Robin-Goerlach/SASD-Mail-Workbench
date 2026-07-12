using System;
using System.Security.Cryptography;
using System.Text;
using Sasd.MailClient.Application.Abstractions;

namespace Sasd.MailClient.Infrastructure.Services;

/// <summary>
/// Bildet einen SHA-256-Fingerprint ueber den Rohinhalt.
/// Fuer den ersten Wurf ist das robust, einfach und nachvollziehbar.
/// </summary>
public sealed class Sha256MessageFingerprintService : IMessageFingerprintService
{
    public string CreateFingerprint(string rawContent)
    {
        byte[] contentBytes = Encoding.UTF8.GetBytes(rawContent);
        byte[] hashBytes = SHA256.HashData(contentBytes);
        return Convert.ToHexString(hashBytes);
    }
}
