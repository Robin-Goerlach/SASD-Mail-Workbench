using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Application.Abstractions;

/// <summary>
/// Berechnet Fingerprints direkt aus unveränderten Byte-Streams.
/// </summary>
/// <remarks>
/// Eine Rohnachricht darf vor der Hashbildung nicht als Text dekodiert werden.
/// Andernfalls könnten Zeichencodierung, Zeilenenden oder binäre MIME-Teile
/// verändert werden und die Deduplizierung wäre nicht mehr bytegenau.
/// </remarks>
public interface IMessageFingerprintService
{
    /// <summary>
    /// Liest den Quellstream vollständig und berechnet SHA-256 sowie Byte-Länge.
    /// </summary>
    /// <param name="source">Lesbarer Stream mit den unveränderten Nachrichtendaten.</param>
    /// <param name="cancellationToken">Token zum kontrollierten Abbruch.</param>
    /// <returns>Der bytegenaue Fingerprint.</returns>
    Task<MessageFingerprint> ComputeAsync(
        Stream source,
        CancellationToken cancellationToken);

    /// <summary>
    /// Kopiert den Quellstream in den Zielstream und berechnet währenddessen den Fingerprint.
    /// </summary>
    /// <param name="source">Lesbarer Eingabestream.</param>
    /// <param name="destination">Beschreibbarer Zielstream.</param>
    /// <param name="cancellationToken">Token zum kontrollierten Abbruch.</param>
    /// <returns>Fingerprint der tatsächlich kopierten Bytes.</returns>
    Task<MessageFingerprint> CopyAndComputeAsync(
        Stream source,
        Stream destination,
        CancellationToken cancellationToken);
}
