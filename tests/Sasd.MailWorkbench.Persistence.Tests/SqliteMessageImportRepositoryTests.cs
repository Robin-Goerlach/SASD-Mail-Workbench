using NUnit.Framework;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Models;
using Sasd.MailWorkbench.Domain.ValueObjects;
using Sasd.MailWorkbench.Persistence.Repositories;
using Sasd.MailWorkbench.Tests.Support;

namespace Sasd.MailWorkbench.Persistence.Tests;

[TestFixture]
public sealed class SqliteMessageImportRepositoryTests
{
    [Test]
    public async Task InitializeAsync_IsIdempotentAndStoresRelativePath()
    {
        using TemporaryWorkspace workspace = new();
        string db = workspace.PathFor("catalog", "test.db");
        SqliteMessageImportRepository repository = new(db);
        await repository.InitializeAsync(CancellationToken.None);
        await repository.InitializeAsync(CancellationToken.None);

        DateTimeOffset now = new(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);
        MessageFingerprint fingerprint = new(new string('A', 64), 42);
        ImportAttempt attempt = CreateAttempt("attempt-1", "account-1", "uid-1", fingerprint, now);
        await CreateThroughCommittingAsync(repository, attempt);

        StoredRawMessage message = new("message-1", "account-1", fingerprint, "raw-mails/2026/07/message-1.eml", now);
        SourceObservation observation = new("account-1", "uid-1", message.Id, now, now);
        var result = await repository.CompleteNewMessageAsync(attempt, message, observation, CancellationToken.None);
        var loaded = await repository.FindBySourceKeyAsync("account-1", "uid-1", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CandidateWasCreated, Is.True);
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded!.RelativePath, Is.EqualTo("raw-mails/2026/07/message-1.eml"));
        });
    }

    [Test]
    public async Task CompleteNewMessageAsync_DeduplicatesConcurrentCandidate()
    {
        using TemporaryWorkspace workspace = new();
        SqliteMessageImportRepository repository = new(workspace.PathFor("catalog", "test.db"));
        await repository.InitializeAsync(CancellationToken.None);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        MessageFingerprint fingerprint = new(new string('B', 64), 100);

        ImportAttempt firstAttempt = CreateAttempt("attempt-a", "account", "source-a", fingerprint, now);
        await CreateThroughCommittingAsync(repository, firstAttempt);
        StoredRawMessage first = new("message-a", "account", fingerprint, "raw-mails/a.eml", now);
        await repository.CompleteNewMessageAsync(firstAttempt, first, new SourceObservation("account", "source-a", first.Id, now, now), CancellationToken.None);

        ImportAttempt secondAttempt = CreateAttempt("attempt-b", "account", "source-b", fingerprint, now);
        await CreateThroughCommittingAsync(repository, secondAttempt);
        StoredRawMessage second = new("message-b", "account", fingerprint, "raw-mails/b.eml", now);
        var result = await repository.CompleteNewMessageAsync(secondAttempt, second, new SourceObservation("account", "source-b", second.Id, now, now), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.CandidateWasCreated, Is.False);
            Assert.That(result.CanonicalMessage.Id, Is.EqualTo("message-a"));
        });
    }

    private static ImportAttempt CreateAttempt(
        string id,
        string accountId,
        string sourceKey,
        MessageFingerprint fingerprint,
        DateTimeOffset now) => new()
    {
        Id = id,
        AccountId = accountId,
        SourceKey = sourceKey,
        State = ImportAttemptState.Committing,
        StagingRelativePath = $"staging/{id}.part",
        TargetRelativePath = $"raw-mails/{id}.eml",
        Fingerprint = fingerprint,
        StartedAtUtc = now,
        UpdatedAtUtc = now
    };

    private static async Task CreateThroughCommittingAsync(
        SqliteMessageImportRepository repository,
        ImportAttempt finalAttempt)
    {
        ImportAttempt discovered = finalAttempt with
        {
            State = ImportAttemptState.Discovered,
            Fingerprint = null,
            StagingRelativePath = null,
            TargetRelativePath = null
        };
        await repository.CreateAttemptAsync(discovered, CancellationToken.None);
        await repository.UpdateAttemptAsync(discovered with
        {
            State = ImportAttemptState.Downloading
        }, CancellationToken.None);
        await repository.UpdateAttemptAsync(finalAttempt with
        {
            State = ImportAttemptState.Staged,
            TargetRelativePath = null
        }, CancellationToken.None);
        await repository.UpdateAttemptAsync(finalAttempt, CancellationToken.None);
    }
}
