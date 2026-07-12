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

        Assert.Multiple(() =>
        {
            Assert.That(firstFingerprint.Sha256, Is.Not.EqualTo(secondFingerprint.Sha256));
            Assert.That(firstFingerprint.LengthInBytes, Is.EqualTo(crlf.Length));
            Assert.That(secondFingerprint.LengthInBytes, Is.EqualTo(lf.Length));
        });
    }

    [Test]
    public async Task CopyAndComputeAsync_CopiesExactly()
    {
        Sha256MessageFingerprintService service = new();
        byte[] bytes = Enumerable.Range(0, 1_000_000).Select(index => (byte)(index % 251)).ToArray();
        await using MemoryStream source = new(bytes);
        await using MemoryStream destination = new();

        var fingerprint = await service.CopyAndComputeAsync(source, destination, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(destination.ToArray(), Is.EqualTo(bytes));
            Assert.That(fingerprint.LengthInBytes, Is.EqualTo(bytes.Length));
        });
    }
}
