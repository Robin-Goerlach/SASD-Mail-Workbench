using System.Text;
using Sasd.MailWorkbench.Application.Contracts.Import;
using Sasd.MailWorkbench.Application.UseCases;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Infrastructure.Logging;
using Sasd.MailWorkbench.Infrastructure.Services;
using Sasd.MailWorkbench.Infrastructure.Sources;
using Sasd.MailWorkbench.Persistence.Repositories;
using Sasd.MailWorkbench.Persistence.Storage;

string root = Path.Combine(AppContext.BaseDirectory, "runtime-data");
string inbox = Path.Combine(root, "samples", "inbox");
string profile = Path.Combine(root, "profile");
string database = Path.Combine(profile, "catalog", "mail-workbench.db");
Directory.CreateDirectory(inbox);
Directory.CreateDirectory(profile);

await SeedAsync(inbox);

Sha256MessageFingerprintService fingerprint = new();
FileSystemRawMessageStore store = new(profile, fingerprint);
SqliteMessageImportRepository repository = new(database);
SystemClock clock = new();
ConsoleApplicationLogger logger = new();
DirectoryRawMessageSource source = new(inbox);

RecoverInterruptedImportsUseCase recovery = new(store, repository, clock, logger);
await recovery.ExecuteAsync(CancellationToken.None);

ImportRawMessagesUseCase importer = new(source, store, repository, clock, logger);
ImportRunReport first = await importer.ExecuteAsync(
    new ImportRawMessagesRequest("demo-account", ImportMode.Incremental),
    CancellationToken.None);
ImportRunReport verification = await importer.ExecuteAsync(
    new ImportRawMessagesRequest("demo-account", ImportMode.CompleteVerification),
    CancellationToken.None);

Print("Inkrementeller Lauf", first);
Print("Vollständige Verifikation", verification);

static void Print(string title, ImportRunReport report)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));
    Console.WriteLine($"Ergebnis:                  {report.Outcome}");
    Console.WriteLine($"Sichtbar:                  {report.Discovered}");
    Console.WriteLine($"Neue Rohnachrichten:       {report.NewlyStored}");
    Console.WriteLine($"Exakte Duplikate:          {report.ExactDuplicates}");
    Console.WriteLine($"Bekannte Quellschlüssel:   {report.SkippedKnownSourceKeys}");
    Console.WriteLine($"Fehler:                    {report.Failed}");
}

static async Task SeedAsync(string inbox)
{
    byte[] first = Encoding.UTF8.GetBytes(
        "From: demo@example.test\r\n" +
        "To: sasd@example.test\r\n" +
        "Subject: SASD Mail Workbench Demo\r\n" +
        "Message-ID: <demo-001@example.test>\r\n" +
        "Content-Type: text/plain; charset=utf-8\r\n\r\n" +
        "Project: SASD Mail Workbench\r\n");
    byte[] second = Encoding.UTF8.GetBytes(
        "From: monitor@example.test\n" +
        "To: sasd@example.test\n" +
        "Subject: Monitoring sample\n" +
        "Message-ID: <demo-002@example.test>\n" +
        "Content-Type: text/plain; charset=utf-8\n\n" +
        "Status: OK\n");

    await File.WriteAllBytesAsync(Path.Combine(inbox, "001-demo.eml"), first);
    await File.WriteAllBytesAsync(Path.Combine(inbox, "002-monitoring.eml"), second);
}
