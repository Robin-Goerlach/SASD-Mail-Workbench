using System.Text;
using NUnit.Framework;
using Sasd.MailWorkbench.Infrastructure.Services;

namespace Sasd.MailWorkbench.Infrastructure.Tests;

[TestFixture]
public sealed class Sha256MessageFingerprintServiceTests
{
    [Test]
    public async Task ComputeAsync_PreservesByteIdentity()
    {
        Sha256MessageFingerprintService service = new();
        byte[] crlf = Encoding.UTF8.GetBytes("A\r\nB\r\n");
        byte[] lf = Encoding.UTF8.GetBytes("A\nB\n");

        await using MemoryStream first = new(crlf);
        await using MemoryStream second = new(lf);
        var firstFingerprint = await service.ComputeAsync(first, CancellationToken.None);
        var secondFingerprint = await service.ComputeAsync(second, CancellationToken.None);

        Assert.Multiple((Action)(() =>
        {
            Assert.That(firstFingerprint.Sha256, Is.Not.EqualTo(secondFingerprint.Sha256));
            Assert.That(firstFingerprint.LengthInBytes, Is.EqualTo(crlf.Length));
            Assert.That(secondFingerprint.LengthInBytes, Is.EqualTo(lf.Length));
        }));
    }

    [Test]
    public async Task CopyAndComputeAsync_CopiesExactly()
    {
        Sha256MessageFingerprintService service = new();
        byte[] bytes = Enumerable.Range(0, 1_000_000).Select(index => (byte)(index % 251)).ToArray();
        await using MemoryStream source = new(bytes);
        await using MemoryStream destination = new();

        var fingerprint = await service.CopyAndComputeAsync(source, destination, CancellationToken.None);

        Assert.Multiple((Action)(() =>
        {
            Assert.That(destination.ToArray(), Is.EqualTo(bytes));
            Assert.That(fingerprint.LengthInBytes, Is.EqualTo(bytes.Length));
        }));
    }

    [Test]
    public async Task CopyAndComputeAsync_AcceptsForwardOnlyChunkedSource()
    {
        Sha256MessageFingerprintService service = new();
        byte[] bytes = Enumerable.Range(0, 256_003).Select(index => (byte)(index % 239)).ToArray();

        // Ein POP3-Transport darf später einen echten Netzwerkstream liefern.
        // Dieser Test stellt deshalb sicher, dass der Fingerprint-/Stagingpfad
        // weder Seek noch Length noch einen vollständig gepufferten Quellstream
        // voraussetzt. Kleine Chunks simulieren fragmentierte Netzwerklesevorgänge.
        await using ForwardOnlyChunkedStream source = new(bytes, maximumChunkSize: 37);
        await using MemoryStream destination = new();

        var fingerprint = await service.CopyAndComputeAsync(
            source,
            destination,
            CancellationToken.None);

        Assert.Multiple((Action)(() =>
        {
            Assert.That(source.CanSeek, Is.False);
            Assert.That(destination.ToArray(), Is.EqualTo(bytes));
            Assert.That(fingerprint.LengthInBytes, Is.EqualTo(bytes.Length));
        }));
    }

    private sealed class ForwardOnlyChunkedStream : Stream
    {
        private readonly MemoryStream _inner;
        private readonly int _maximumChunkSize;

        public ForwardOnlyChunkedStream(byte[] bytes, int maximumChunkSize)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumChunkSize);

            _inner = new MemoryStream(bytes, writable: false);
            _maximumChunkSize = maximumChunkSize;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, Math.Min(count, _maximumChunkSize));
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            int count = Math.Min(buffer.Length, _maximumChunkSize);
            return _inner.ReadAsync(buffer[..count], cancellationToken);
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return _inner.ReadAsync(
                buffer,
                offset,
                Math.Min(count, _maximumChunkSize),
                cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}
