using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Application.UseCases;
using Sasd.MailClient.Bootstrap.ConsoleApp;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;
using Sasd.MailClient.Infrastructure.Logging;
using Sasd.MailClient.Infrastructure.Mail;
using Sasd.MailClient.Infrastructure.Parsing;
using Sasd.MailClient.Infrastructure.Processing;
using Sasd.MailClient.Infrastructure.Services;
using Sasd.MailClient.Infrastructure.Storage;

internal static class Program
{
    /// <summary>
    /// Konsolenstartpunkt fuer den technischen Projektstart.
    /// Die App zeigt bewusst zuerst den lauffaehigen Kern ohne GUI.
    /// </summary>
    private static async Task<int> Main(string[] args)
    {
        CancellationToken cancellationToken = CancellationToken.None;

        string solutionRoot = ResolveSolutionRoot();
        string workspaceRoot = Path.Combine(solutionRoot, "runtime-data");
        string sampleInboxDirectory = Path.Combine(solutionRoot, "samples", "mails", "inbox");

        AppPathLayout pathLayout = new AppPathLayout(workspaceRoot);

        Directory.CreateDirectory(pathLayout.RootDirectory);
        Directory.CreateDirectory(pathLayout.DatabaseDirectory);
        Directory.CreateDirectory(pathLayout.RawMailDirectory);
        Directory.CreateDirectory(pathLayout.TempDirectory);

        // Damit der Erststart fuer einen Entwickler wirklich direkt klappt,
        // erzeugen wir bei Bedarf selbst kleine Demo-Mails.
        await SampleMailSeeder.EnsureSampleMailsExistAsync(sampleInboxDirectory);

        IApplicationLogger logger = new ConsoleApplicationLogger();
        IClock clock = new SystemClock();
        IMessageFingerprintService fingerprintService = new Sha256MessageFingerprintService();

        IMailSource mailSource = new DirectoryMailSource(sampleInboxDirectory);
        IRawMessageStore rawMessageStore = new FileSystemRawMessageStore(pathLayout.RawMailDirectory);
        IMessageCatalogRepository catalogRepository = new SqliteMessageCatalogRepository(pathLayout.DatabaseFilePath);
        IMessageParser messageParser = new SimpleMimeMessageParser();

        IReadOnlyList<IMessageProcessor> processors = new List<IMessageProcessor>
        {
            new NormalizeMessageProcessor(),
            new BasicTaggingProcessor(),
            new KeyValueBodyExtractionProcessor()
        };

        MailAccount account = new MailAccount
        {
            DisplayName = "Lokale Demoquelle",
            Host = "samples.local",
            Port = 0,
            SecurityMode = MailSecurityMode.None,
            UserName = "demo-user",
            Description = "Lokale Demo-Mailquelle fuer den technischen Projektstart."
        };

        FetchAndProcessMessagesUseCase useCase = new FetchAndProcessMessagesUseCase(
            mailSource,
            rawMessageStore,
            catalogRepository,
            messageParser,
            processors,
            fingerprintService,
            clock,
            logger);

        FetchRunReport report = await useCase.ExecuteAsync(account, cancellationToken);

        Console.WriteLine();
        Console.WriteLine("=== Zusammenfassung ===");
        Console.WriteLine($"Gefundene Remote-Mails : {report.DiscoveredRemoteMessages}");
        Console.WriteLine($"Neu importierte Mails  : {report.NewlyImportedMessages}");
        Console.WriteLine($"Bereits bekannte Mails : {report.SkippedAlreadyKnownMessages}");

        if (report.Warnings.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Warnungen:");
            foreach (string warning in report.Warnings)
            {
                Console.WriteLine($"- {warning}");
            }
        }

        if (report.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Fehler:");
            foreach (string error in report.Errors)
            {
                Console.WriteLine($"- {error}");
            }
        }

        IReadOnlyList<ParsedMessage> parsedMessages = await catalogRepository.ListParsedMessagesAsync(cancellationToken);

        Console.WriteLine();
        Console.WriteLine("=== Lokale Nachrichten ===");

        foreach (ParsedMessage parsedMessage in parsedMessages)
        {
            Console.WriteLine($"ID      : {parsedMessage.Id}");
            Console.WriteLine($"Betreff : {parsedMessage.Subject}");
            Console.WriteLine($"Von     : {parsedMessage.From}");
            Console.WriteLine($"Tags    : {(parsedMessage.Tags.Count == 0 ? "(keine)" : string.Join(", ", parsedMessage.Tags))}");

            IReadOnlyList<ProcessingResult> results =
                await catalogRepository.ListProcessingResultsAsync(parsedMessage.Id, cancellationToken);

            foreach (ProcessingResult result in results)
            {
                Console.WriteLine($"  Prozessor: {result.ProcessorName} | Erfolg: {result.Success}");

                foreach (KeyValuePair<string, string> extractedEntry in result.ExtractedData)
                {
                    Console.WriteLine($"    {extractedEntry.Key}: {extractedEntry.Value}");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine($"Arbeitsverzeichnis: {workspaceRoot}");
        Console.WriteLine($"SQLite-Datenbank : {pathLayout.DatabaseFilePath}");
        Console.WriteLine("Projektstart erfolgreich abgeschlossen.");

        return report.Errors.Count > 0 ? 1 : 0;
    }

    /// <summary>
    /// Bestimmt das Solution-Root relativ zur laufenden Assembly.
    /// Der Einstieg soll moeglichst robust gegen unterschiedliche Startorte sein.
    /// </summary>
    private static string ResolveSolutionRoot()
    {
        string current = AppContext.BaseDirectory;
        DirectoryInfo? directory = new DirectoryInfo(current);

        while (directory is not null)
        {
            string potentialSolutionFile = Path.Combine(directory.FullName, "Sasd.MailClient.sln");

            if (File.Exists(potentialSolutionFile))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        // Fallback fuer ungewoehnliche Startorte:
        // In diesem Fall arbeiten wir einfach relativ zum aktuellen Arbeitsverzeichnis.
        return Directory.GetCurrentDirectory();
    }
}
