using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sasd.MailClient.Domain.Models;
using Sasd.MailClient.Infrastructure.Storage;
using Sasd.MailClient.Tests.TestSupport;
using Sasd.MailClient.Tests.Testing;

namespace Sasd.MailClient.Tests.Suites;

/// <summary>
/// Prueft die Speicherbausteine des ersten Wurfs.
/// Gerade hier ist Verlaesslichkeit wichtig, weil Rohdaten und SQLite-Katalog die Basis
/// fuer spaetere Erweiterungen bilden.
/// </summary>
public static class StorageTests
{
    public static IEnumerable<TestCaseDefinition> GetCases()
    {
        yield return new TestCaseDefinition
        {
            Name = "Dateisystem-Rohspeicher speichert und laedt Inhalte wieder",
            ExecuteAsync = RawMessageStoreRoundtripAsync
        };

        yield return new TestCaseDefinition
        {
            Name = "SQLite-Katalog findet Nachrichten ueber ExternalKey wieder",
            ExecuteAsync = SqliteCatalogFindsExistingMessageAsync
        };

        yield return new TestCaseDefinition
        {
            Name = "SQLite-Katalog speichert Parsing- und Prozessorergebnisse nachvollziehbar",
            ExecuteAsync = SqliteCatalogStoresParsedMessagesAndProcessingResultsAsync
        };
    }

    private static async Task RawMessageStoreRoundtripAsync()
    {
        using TestWorkspace workspace = new TestWorkspace("raw-message-store-roundtrip");

        FileSystemRawMessageStore store = new FileSystemRawMessageStore(workspace.RawMailDirectory);

        RawMessage message = new RawMessage
        {
            Id = "raw-1",
            AccountId = "account-1"
        };

        string content = TestMailFactory.CreatePlainTextProjectMail();
        string path = await store.SaveAsync(message, content, CancellationToken.None);

        message.StoragePath = path;

        string loaded = await store.LoadAsync(message, CancellationToken.None);

        AssertEx.True(System.IO.File.Exists(path), "Die Rohmail-Datei wurde nicht erzeugt.");
        AssertEx.Equal(content, loaded, "Der geladene Rohinhalt stimmt nicht mit dem gespeicherten Inhalt ueberein.");
    }

    private static async Task SqliteCatalogFindsExistingMessageAsync()
    {
        using TestWorkspace workspace = new TestWorkspace("sqlite-catalog-find-existing");

        SqliteMessageCatalogRepository repository = new SqliteMessageCatalogRepository(workspace.DatabaseFilePath);

        RawMessage rawMessage = new RawMessage
        {
            Id = "raw-2",
            AccountId = "account-77",
            ExternalMessageKey = "001-message.eml",
            StoragePath = "dummy.eml"
        };

        await repository.SaveRawMessageAsync(rawMessage, CancellationToken.None);

        bool exists = await repository.ExistsByExternalKeyAsync(
            "account-77",
            "001-message.eml",
            CancellationToken.None);

        RawMessage? loaded = await repository.GetRawMessageAsync("raw-2", CancellationToken.None);

        AssertEx.True(exists, "Der SQLite-Katalog hat die zuvor gespeicherte Nachricht nicht wiedergefunden.");
        AssertEx.NotNull(loaded, "Die gespeicherte Rohnachricht konnte nicht wieder geladen werden.");
        AssertEx.Equal("001-message.eml", loaded!.ExternalMessageKey, "Die geladene Rohnachricht enthaelt einen falschen ExternalKey.");
    }


    private static async Task SqliteCatalogStoresParsedMessagesAndProcessingResultsAsync()
    {
        using TestWorkspace workspace = new TestWorkspace("sqlite-catalog-parsed-results");

        SqliteMessageCatalogRepository repository = new SqliteMessageCatalogRepository(workspace.DatabaseFilePath);

        RawMessage rawMessage = new RawMessage
        {
            Id = "parsed-1",
            AccountId = "account-88",
            ExternalMessageKey = "sqlite-test-1.eml",
            StoragePath = "sqlite-test-1.eml",
            SourceIdentifier = "sqlite-test-source",
            Fingerprint = "fingerprint-test-1"
        };

        ParsedMessage parsedMessage = new ParsedMessage
        {
            Id = rawMessage.Id,
            AccountId = rawMessage.AccountId,
            HeaderMessageId = "<parsed-1@example.test>",
            Subject = "Stored in SQLite",
            From = new MailAddress
            {
                DisplayName = "Example Sender",
                Address = "sender@example.test"
            },
            NormalizedText = "Invoice: INV-88",
            TextBody = "Invoice: INV-88",
            Tags = new List<string> { "finance", "sqlite" }
        };

        ProcessingResult processingResult = new ProcessingResult
        {
            MessageId = parsedMessage.Id,
            ProcessorName = "DemoProcessor",
            Success = true,
            Tags = new List<string> { "finance" },
            ExtractedData = new Dictionary<string, string>
            {
                ["Invoice"] = "INV-88"
            },
            Notes = new List<ProcessingNote>
            {
                new ProcessingNote
                {
                    Level = Sasd.MailClient.Domain.Enums.ProcessingNoteLevel.Information,
                    Source = "DemoProcessor",
                    Message = "Testnote"
                }
            }
        };

        await repository.SaveRawMessageAsync(rawMessage, CancellationToken.None);
        await repository.SaveParsedMessageAsync(parsedMessage, CancellationToken.None);
        await repository.SaveProcessingResultAsync(processingResult, CancellationToken.None);

        IReadOnlyList<ParsedMessage> parsedMessages = await repository.ListParsedMessagesAsync(CancellationToken.None);
        IReadOnlyList<ProcessingResult> processingResults = await repository.ListProcessingResultsAsync(parsedMessage.Id, CancellationToken.None);

        AssertEx.Equal(1, parsedMessages.Count, "Der SQLite-Katalog sollte genau eine geparste Nachricht gespeichert haben.");
        AssertEx.Equal("Stored in SQLite", parsedMessages[0].Subject, "Der Betreff der geparsten Nachricht wurde nicht korrekt gelesen.");
        AssertEx.True(parsedMessages[0].Tags.Contains("sqlite"), "Das gespeicherte Tag 'sqlite' wurde nicht wieder geladen.");
        AssertEx.Equal(1, processingResults.Count, "Es sollte genau ein Prozessorergebnis vorliegen.");
        AssertEx.Equal("INV-88", processingResults[0].ExtractedData["Invoice"], "Der extrahierte Invoice-Wert wurde nicht korrekt wieder geladen.");
        AssertEx.True(System.IO.File.Exists(workspace.DatabaseFilePath), "Die SQLite-Datenbankdatei wurde nicht erzeugt.");
    }

}
