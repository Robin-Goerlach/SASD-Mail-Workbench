using System.Text;
using NUnit.Framework;
using Sasd.MailWorkbench.Application.Contracts.Import;
using Sasd.MailWorkbench.Application.Tests.Support;
using Sasd.MailWorkbench.Application.UseCases;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Infrastructure.Services;
using Sasd.MailWorkbench.Infrastructure.Sources;
using Sasd.MailWorkbench.Persistence.Repositories;
using Sasd.MailWorkbench.Persistence.Storage;
using Sasd.MailWorkbench.Tests.Support;

namespace Sasd.MailWorkbench.Application.Tests;

[TestFixture]
public sealed class ImportRawMessagesUseCaseTests
{
    [Test]
    public async Task CompleteVerification_DiscardsExactDuplicatesAutomatically()
    {
        using TemporaryWorkspace workspace = new();
        string inbox = workspace.PathFor("inbox", "placeholder");
        inbox = Path.GetDirectoryName(inbox)!;
        byte[] mail = Encoding.UTF8.GetBytes("Subject: Test\r\n\r\nBody\r\n");
        await File.WriteAllBytesAsync(Path.Combine(inbox, "one.eml"), mail);
        await File.WriteAllBytesAsync(Path.Combine(inbox, "two.eml"), mail);

        var useCase = Build(workspace, inbox, out SqliteMessageImportRepository repository);
        ImportRunReport report = await useCase.ExecuteAsync(
            new ImportRawMessagesRequest("account", ImportMode.CompleteVerification),
            CancellationToken.None);
        var messages = await repository.ListMessagesAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(report.NewlyStored, Is.EqualTo(1));
            Assert.That(report.ExactDuplicates, Is.EqualTo(1));
            Assert.That(messages, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Incremental_SecondRunSkipsKnownSourceKey()
    {
        using TemporaryWorkspace workspace = new();
        string inbox = workspace.PathFor("inbox", "placeholder");
        inbox = Path.GetDirectoryName(inbox)!;
        await File.WriteAllTextAsync(Path.Combine(inbox, "one.eml"), "Subject: Test\n\nBody\n");
        var useCase = Build(workspace, inbox, out _);

        ImportRunReport first = await useCase.ExecuteAsync(
            new ImportRawMessagesRequest("account", ImportMode.Incremental),
            CancellationToken.None);
        ImportRunReport second = await useCase.ExecuteAsync(
            new ImportRawMessagesRequest("account", ImportMode.Incremental),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first.NewlyStored, Is.EqualTo(1));
            Assert.That(second.NewlyStored, Is.Zero);
            Assert.That(second.SkippedKnownSourceKeys, Is.EqualTo(1));
        });
    }

    private static ImportRawMessagesUseCase Build(
        TemporaryWorkspace workspace,
        string inbox,
        out SqliteMessageImportRepository repository)
    {
        Sha256MessageFingerprintService fingerprint = new();
        string profilePlaceholder = workspace.PathFor("profile", "placeholder");
        string profile = Path.GetDirectoryName(profilePlaceholder)!;
        FileSystemRawMessageStore store = new(profile, fingerprint);
        repository = new SqliteMessageImportRepository(workspace.PathFor("catalog", "test.db"));
        return new ImportRawMessagesUseCase(
            new DirectoryRawMessageSource(inbox),
            store,
            repository,
            new FixedClock(new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero)),
            new TestLogger());
    }
}
