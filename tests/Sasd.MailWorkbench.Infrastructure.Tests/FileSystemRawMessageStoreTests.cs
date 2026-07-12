using NUnit.Framework;
using Sasd.MailWorkbench.Infrastructure.Services;
using Sasd.MailWorkbench.Persistence.Storage;
using Sasd.MailWorkbench.Tests.Support;

namespace Sasd.MailWorkbench.Infrastructure.Tests;

[TestFixture]
public sealed class FileSystemRawMessageStoreTests
{
    [Test]
    public async Task StageAndCommit_PreserveBytesAndRelativePaths()
    {
        using TemporaryWorkspace workspace = new();
        FileSystemRawMessageStore store = new(workspace.Root, new Sha256MessageFingerprintService());
        byte[] original = [0, 1, 2, 13, 10, 255, 128, 42];
        await using MemoryStream input = new(original);

        var staged = await store.StageAsync(input, CancellationToken.None);
        string target = store.GetTargetRelativePath("message-1", new DateTimeOffset(2026, 7, 12, 0, 0, 0, TimeSpan.Zero));
        await store.CommitAsync(staged.StagingRelativePath, target, CancellationToken.None);
        await using Stream stored = await store.OpenReadAsync(target, CancellationToken.None);
        await using MemoryStream copy = new();
        await stored.CopyToAsync(copy);

        Assert.Multiple(() =>
        {
            Assert.That(target, Is.EqualTo("raw-mails/2026/07/message-1.eml"));
            Assert.That(copy.ToArray(), Is.EqualTo(original));
            Assert.That(staged.Fingerprint.LengthInBytes, Is.EqualTo(original.Length));
        });
    }

    [Test]
    public void OpenReadAsync_RejectsPathTraversal()
    {
        using TemporaryWorkspace workspace = new();
        FileSystemRawMessageStore store = new(workspace.Root, new Sha256MessageFingerprintService());
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await store.OpenReadAsync("../outside.eml", CancellationToken.None);
        });
    }
}
