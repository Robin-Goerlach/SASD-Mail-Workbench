using System.Text;
using NUnit.Framework;
using Sasd.MailWorkbench.Application.Tests.Support;
using Sasd.MailWorkbench.Application.UseCases;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Models;
using Sasd.MailWorkbench.Infrastructure.Services;
using Sasd.MailWorkbench.Persistence.Repositories;
using Sasd.MailWorkbench.Persistence.Storage;
using Sasd.MailWorkbench.Tests.Support;

namespace Sasd.MailWorkbench.Application.Tests;

[TestFixture]
public sealed class RecoverInterruptedImportsUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_CompletesStagedImport()
    {
        using TemporaryWorkspace workspace = new();
        Sha256MessageFingerprintService fingerprintService = new();
        string profile = workspace.PathFor("profile", "placeholder");
        profile = Path.GetDirectoryName(profile)!;
        FileSystemRawMessageStore store = new(profile, fingerprintService);
        SqliteMessageImportRepository repository = new(workspace.PathFor("catalog", "test.db"));
        await repository.InitializeAsync(CancellationToken.None);

        await using MemoryStream raw = new(Encoding.UTF8.GetBytes("Subject: Recovery\r\n\r\nBody"));
        StagedRawMessage staged = await store.StageAsync(raw, CancellationToken.None);
        DateTimeOffset now = new(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);
        ImportAttempt attempt = new()
        {
            Id = "recovery-1",
            AccountId = "account",
            SourceKey = "uid-recovery",
            State = ImportAttemptState.Discovered,
            StartedAtUtc = now,
            UpdatedAtUtc = now
        };
        await repository.CreateAttemptAsync(attempt, CancellationToken.None);
        await repository.UpdateAttemptAsync(attempt with { State = ImportAttemptState.Downloading }, CancellationToken.None);
        await repository.UpdateAttemptAsync(attempt with
        {
            State = ImportAttemptState.Staged,
            StagingRelativePath = staged.StagingRelativePath,
            Fingerprint = staged.Fingerprint
        }, CancellationToken.None);

        RecoverInterruptedImportsUseCase recovery = new(
            store,
            repository,
            new FixedClock(now),
            new TestLogger());
        var report = await recovery.ExecuteAsync(CancellationToken.None);
        var messages = await repository.ListMessagesAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.Recovered, Is.EqualTo(1));
            Assert.That(messages, Has.Count.EqualTo(1));
            Assert.That(messages[0].RelativePath, Does.StartWith("raw-mails/2026/07/"));
        });
    }

    [Test]
    public async Task ExecuteAsync_RemovesUnreferencedStagingFile()
    {
        using TemporaryWorkspace workspace = new();
        Sha256MessageFingerprintService fingerprintService = new();
        string profile = Path.GetDirectoryName(workspace.PathFor("profile", "placeholder"))!;
        FileSystemRawMessageStore store = new(profile, fingerprintService);
        SqliteMessageImportRepository repository = new(workspace.PathFor("catalog", "test.db"));
        await repository.InitializeAsync(CancellationToken.None);

        await using MemoryStream raw = new(Encoding.UTF8.GetBytes("Subject: Orphan\r\n\r\nBody"));
        StagedRawMessage staged = await store.StageAsync(raw, CancellationToken.None);

        RecoverInterruptedImportsUseCase recovery = new(
            store,
            repository,
            new FixedClock(new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero)),
            new TestLogger());

        var report = await recovery.ExecuteAsync(CancellationToken.None);
        bool stillExists = await store.ExistsAsync(staged.StagingRelativePath, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.OrphanStagingFilesDiscarded, Is.EqualTo(1));
            Assert.That(stillExists, Is.False);
        });
    }

}
