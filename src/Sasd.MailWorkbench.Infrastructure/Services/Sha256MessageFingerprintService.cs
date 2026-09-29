using System.Security.Cryptography;
using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Infrastructure.Services;

/// <summary>
/// Berechnet SHA-256 über die tatsächlich gelesenen Bytes, ohne Textdekodierung
/// oder Zeilenendennormalisierung.
/// </summary>
public sealed class Sha256MessageFingerprintService : IMessageFingerprintService
{
    private const int BufferSize = 128 * 1024;

    /// <inheritdoc />
    public async Task<MessageFingerprint> ComputeAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        return await CopyCoreAsync(source, destination: null, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MessageFingerprint> CopyAndComputeAsync(
        Stream source,
        Stream destination,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
        {
            throw new ArgumentException("Der Zielstream muss beschreibbar sein.", nameof(destination));
        }

        return await CopyCoreAsync(source, destination, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<MessageFingerprint> CopyCoreAsync(
        Stream source,
        Stream? destination,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
        {
            throw new ArgumentException("Der Quellstream muss lesbar sein.", nameof(source));
        }

        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] buffer = new byte[BufferSize];
        long total = 0;

        while (true)
        {
            int read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            hash.AppendData(buffer, 0, read);
            total += read;

            if (destination is not null)
            {
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        string sha256 = Convert.ToHexString(hash.GetHashAndReset());
        return new MessageFingerprint(sha256, total);
    }
}
