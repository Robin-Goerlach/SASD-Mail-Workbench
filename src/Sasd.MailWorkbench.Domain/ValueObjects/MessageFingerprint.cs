using System.Globalization;

namespace Sasd.MailWorkbench.Domain.ValueObjects;

/// <summary>
/// Bytegenauer Fingerprint einer kanonischen Rohnachricht.
/// </summary>
public sealed record MessageFingerprint
{
    /// <summary>
    /// Erstellt einen validierten SHA-256-Fingerprint.
    /// </summary>
    public MessageFingerprint(string sha256, long lengthInBytes)
    {
        if (string.IsNullOrWhiteSpace(sha256))
        {
            throw new ArgumentException("Der SHA-256-Wert darf nicht leer sein.", nameof(sha256));
        }

        string normalized = sha256.Trim().ToUpperInvariant();
        if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
        {
            throw new ArgumentException("Der SHA-256-Wert muss aus genau 64 Hexadezimalzeichen bestehen.", nameof(sha256));
        }

        if (lengthInBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lengthInBytes), "Die Byte-Länge darf nicht negativ sein.");
        }

        Sha256 = normalized;
        LengthInBytes = lengthInBytes;
    }

    /// <summary>
    /// SHA-256 in kanonischer Großschreibung.
    /// </summary>
    public string Sha256 { get; }

    /// <summary>
    /// Länge der unveränderten Rohmail in Bytes.
    /// </summary>
    public long LengthInBytes { get; }

    /// <summary>
    /// Liefert eine kompakte, diagnosefreundliche Darstellung.
    /// </summary>
    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"{Sha256}:{LengthInBytes}");
}
