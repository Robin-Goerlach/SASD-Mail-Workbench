using NUnit.Framework;
using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Application.Contracts.Import;
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
public sealed class ImportFailureClassificationTests
{
    [Test]
    public async Task ExecuteAsync_ClassifiesSourceIoFailureAsDownloadError()
    {
        using TemporaryWorkspace workspace = new();
        Sha256MessageFingerprintService fingerprint = new();
        string profile = Path.GetDirectoryName(workspace.PathFor("profile", "placeholder"))!;
        FileSystemRawMessageStore store = new(profile, fingerprint);
        SqliteMessageImportRepository repository = new(workspace.PathFor("catalog", "test.db"));
        ImportRawMessagesUseCase useCase = new(
            new ThrowingRawMessageSource(),
            store,
            repository,
            new FixedClock(new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero)),
            new TestLogger());

        ImportRunReport report = await useCase.ExecuteAsync(
            new ImportRawMessagesRequest("account", ImportMode.CompleteVerification),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.Outcome, Is.EqualTo(ImportRunOutcome.CompletedWithErrors));
            Assert.That(report.Failed, Is.EqualTo(1));
            Assert.That(report.Errors[0].Code, Is.EqualTo("DOWNLOAD_IO_ERROR"));
        });
    }

    private sealed class ThrowingRawMessageSource : IRawMessageSource
    {
        public Task<IReadOnlyList<RawMessageSourceItem>> ListAsync(
            string accountId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RawMessageSourceItem>>(
                [new RawMessageSourceItem("uid-1", "Fehlerhafte Testquelle")]);

        public Task<Stream> OpenReadAsync(
            string accountId,
            RawMessageSourceItem item,
            CancellationToken cancellationToken) =>
            throw new IOException("Simulierter Lesefehler der Quelle.");
    }
}
