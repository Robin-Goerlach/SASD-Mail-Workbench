using NUnit.Framework;
using Sasd.MailWorkbench.Domain.ValueObjects;

namespace Sasd.MailWorkbench.Domain.Tests;

[TestFixture]
public sealed class MessageFingerprintTests
{
    [Test]
    public void Constructor_NormalizesLowercaseHash()
    {
        MessageFingerprint fingerprint = new(new string('a', 64), 123);
        Assert.That(fingerprint.Sha256, Is.EqualTo(new string('A', 64)));
        Assert.That(fingerprint.LengthInBytes, Is.EqualTo(123));
    }

    [TestCase("")]
    [TestCase("ABC")]
    [TestCase("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG")]
    public void Constructor_RejectsInvalidHash(string value)
    {
        Assert.That(() => new MessageFingerprint(value, 0), Throws.ArgumentException);
    }
}
