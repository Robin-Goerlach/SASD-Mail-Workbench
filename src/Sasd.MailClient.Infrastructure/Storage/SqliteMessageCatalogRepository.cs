using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Sasd.MailClient.Application.Abstractions;
using Sasd.MailClient.Domain.Enums;
using Sasd.MailClient.Domain.Models;

namespace Sasd.MailClient.Infrastructure.Storage;

/// <summary>
/// SQLite-basierter Nachrichtenkatalog fuer den lokalen Projektstart.
///
/// Warum diese Implementierung wichtig ist:
/// - Sie ersetzt die fruehere JSON-Ablage durch eine robustere, transaktionssichere Basis.
/// - Sie behaelt die bestehende Repository-Schnittstelle bei, damit der Rest der Anwendung
///   unveraendert mit dem Katalog arbeiten kann.
/// - Sie speichert komplexe Felder wie Header, Tags oder Prozessor-Notizen als JSON-Text in
///   SQLite, damit das Schema lesbar bleibt und trotzdem spaeter ausgebaut werden kann.
///
/// Die Klasse ist bewusst ausfuehrlich kommentiert, damit der Aufbau auch spaeter noch gut
/// nachvollziehbar ist.
/// </summary>
public sealed class SqliteMessageCatalogRepository : IMessageCatalogRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false
    };

    private readonly string _databaseFilePath;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    public SqliteMessageCatalogRepository(string databaseFilePath)
    {
        _databaseFilePath = databaseFilePath;

        string? directory = Path.GetDirectoryName(_databaseFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Pooling wird bewusst deaktiviert, damit lokale Debug- und Testlaeufe einfacher
        // aufraeumen koennen. Gerade bei Dateibasierten Tests spart das haeufiges Kopfzerbrechen.
        SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder
        {
            DataSource = _databaseFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        };

        _connectionString = builder.ToString();
        EnsureSchemaCreated();
    }

    public async Task<bool> ExistsByExternalKeyAsync(
        string accountId,
        string externalKey,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT 1
                FROM raw_messages
                WHERE AccountId = $accountId
                  AND ExternalMessageKey = $externalKey
                LIMIT 1;
                """;

            command.Parameters.AddWithValue("$accountId", accountId);
            command.Parameters.AddWithValue("$externalKey", externalKey);

            object? scalar = await command.ExecuteScalarAsync(cancellationToken);
            return scalar is not null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveRawMessageAsync(
        RawMessage message,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO raw_messages
                (
                    Id,
                    AccountId,
                    ExternalMessageKey,
                    SourceIdentifier,
                    Fingerprint,
                    RetrievedAtUtc,
                    StoragePath,
                    ApproximateSizeInBytes,
                    State
                )
                VALUES
                (
                    $id,
                    $accountId,
                    $externalMessageKey,
                    $sourceIdentifier,
                    $fingerprint,
                    $retrievedAtUtc,
                    $storagePath,
                    $approximateSizeInBytes,
                    $state
                )
                ON CONFLICT(Id) DO UPDATE SET
                    AccountId = excluded.AccountId,
                    ExternalMessageKey = excluded.ExternalMessageKey,
                    SourceIdentifier = excluded.SourceIdentifier,
                    Fingerprint = excluded.Fingerprint,
                    RetrievedAtUtc = excluded.RetrievedAtUtc,
                    StoragePath = excluded.StoragePath,
                    ApproximateSizeInBytes = excluded.ApproximateSizeInBytes,
                    State = excluded.State;
                """;

            command.Parameters.AddWithValue("$id", message.Id);
            command.Parameters.AddWithValue("$accountId", message.AccountId);
            command.Parameters.AddWithValue("$externalMessageKey", message.ExternalMessageKey);
            command.Parameters.AddWithValue("$sourceIdentifier", message.SourceIdentifier);
            command.Parameters.AddWithValue("$fingerprint", message.Fingerprint);
            command.Parameters.AddWithValue("$retrievedAtUtc", ToIsoString(message.RetrievedAtUtc));
            command.Parameters.AddWithValue("$storagePath", message.StoragePath);
            command.Parameters.AddWithValue("$approximateSizeInBytes", message.ApproximateSizeInBytes);
            command.Parameters.AddWithValue("$state", (int)message.State);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<RawMessage?> GetRawMessageAsync(
        string messageId,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    Id,
                    AccountId,
                    ExternalMessageKey,
                    SourceIdentifier,
                    Fingerprint,
                    RetrievedAtUtc,
                    StoragePath,
                    ApproximateSizeInBytes,
                    State
                FROM raw_messages
                WHERE Id = $id
                LIMIT 1;
                """;

            command.Parameters.AddWithValue("$id", messageId);

            await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return MapRawMessage(reader);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveParsedMessageAsync(
        ParsedMessage message,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO parsed_messages
                (
                    Id,
                    AccountId,
                    HeaderMessageId,
                    Subject,
                    FromJson,
                    ToJson,
                    CcJson,
                    ReplyToJson,
                    SentAtUtc,
                    ReceivedAtUtc,
                    TextBody,
                    HtmlBody,
                    HeadersJson,
                    AttachmentsJson,
                    NormalizedText,
                    TagsJson,
                    State
                )
                VALUES
                (
                    $id,
                    $accountId,
                    $headerMessageId,
                    $subject,
                    $fromJson,
                    $toJson,
                    $ccJson,
                    $replyToJson,
                    $sentAtUtc,
                    $receivedAtUtc,
                    $textBody,
                    $htmlBody,
                    $headersJson,
                    $attachmentsJson,
                    $normalizedText,
                    $tagsJson,
                    $state
                )
                ON CONFLICT(Id) DO UPDATE SET
                    AccountId = excluded.AccountId,
                    HeaderMessageId = excluded.HeaderMessageId,
                    Subject = excluded.Subject,
                    FromJson = excluded.FromJson,
                    ToJson = excluded.ToJson,
                    CcJson = excluded.CcJson,
                    ReplyToJson = excluded.ReplyToJson,
                    SentAtUtc = excluded.SentAtUtc,
                    ReceivedAtUtc = excluded.ReceivedAtUtc,
                    TextBody = excluded.TextBody,
                    HtmlBody = excluded.HtmlBody,
                    HeadersJson = excluded.HeadersJson,
                    AttachmentsJson = excluded.AttachmentsJson,
                    NormalizedText = excluded.NormalizedText,
                    TagsJson = excluded.TagsJson,
                    State = excluded.State;
                """;

            command.Parameters.AddWithValue("$id", message.Id);
            command.Parameters.AddWithValue("$accountId", message.AccountId);
            command.Parameters.AddWithValue("$headerMessageId", message.HeaderMessageId);
            command.Parameters.AddWithValue("$subject", message.Subject);
            command.Parameters.AddWithValue("$fromJson", Serialize(message.From));
            command.Parameters.AddWithValue("$toJson", Serialize(message.To));
            command.Parameters.AddWithValue("$ccJson", Serialize(message.Cc));
            command.Parameters.AddWithValue("$replyToJson", Serialize(message.ReplyTo));
            command.Parameters.AddWithValue("$sentAtUtc", message.SentAtUtc.HasValue ? ToIsoString(message.SentAtUtc.Value) : DBNull.Value);
            command.Parameters.AddWithValue("$receivedAtUtc", ToIsoString(message.ReceivedAtUtc));
            command.Parameters.AddWithValue("$textBody", message.TextBody);
            command.Parameters.AddWithValue("$htmlBody", message.HtmlBody);
            command.Parameters.AddWithValue("$headersJson", Serialize(message.Headers));
            command.Parameters.AddWithValue("$attachmentsJson", Serialize(message.Attachments));
            command.Parameters.AddWithValue("$normalizedText", message.NormalizedText);
            command.Parameters.AddWithValue("$tagsJson", Serialize(message.Tags));
            command.Parameters.AddWithValue("$state", (int)message.State);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<ParsedMessage>> ListParsedMessagesAsync(
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            List<ParsedMessage> messages = new List<ParsedMessage>();

            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    Id,
                    AccountId,
                    HeaderMessageId,
                    Subject,
                    FromJson,
                    ToJson,
                    CcJson,
                    ReplyToJson,
                    SentAtUtc,
                    ReceivedAtUtc,
                    TextBody,
                    HtmlBody,
                    HeadersJson,
                    AttachmentsJson,
                    NormalizedText,
                    TagsJson,
                    State
                FROM parsed_messages
                ORDER BY ReceivedAtUtc DESC, Id ASC;
                """;

            await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                messages.Add(MapParsedMessage(reader));
            }

            return messages;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveProcessingResultAsync(
        ProcessingResult result,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO processing_results
                (
                    MessageId,
                    ProcessorName,
                    Success,
                    StartedAtUtc,
                    FinishedAtUtc,
                    TagsJson,
                    ExtractedDataJson,
                    NotesJson
                )
                VALUES
                (
                    $messageId,
                    $processorName,
                    $success,
                    $startedAtUtc,
                    $finishedAtUtc,
                    $tagsJson,
                    $extractedDataJson,
                    $notesJson
                );
                """;

            command.Parameters.AddWithValue("$messageId", result.MessageId);
            command.Parameters.AddWithValue("$processorName", result.ProcessorName);
            command.Parameters.AddWithValue("$success", result.Success ? 1 : 0);
            command.Parameters.AddWithValue("$startedAtUtc", ToIsoString(result.StartedAtUtc));
            command.Parameters.AddWithValue("$finishedAtUtc", ToIsoString(result.FinishedAtUtc));
            command.Parameters.AddWithValue("$tagsJson", Serialize(result.Tags));
            command.Parameters.AddWithValue("$extractedDataJson", Serialize(result.ExtractedData));
            command.Parameters.AddWithValue("$notesJson", Serialize(result.Notes));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<ProcessingResult>> ListProcessingResultsAsync(
        string messageId,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            List<ProcessingResult> results = new List<ProcessingResult>();

            await using SqliteConnection connection = await OpenConnectionAsync(cancellationToken);
            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    MessageId,
                    ProcessorName,
                    Success,
                    StartedAtUtc,
                    FinishedAtUtc,
                    TagsJson,
                    ExtractedDataJson,
                    NotesJson
                FROM processing_results
                WHERE MessageId = $messageId
                ORDER BY StartedAtUtc ASC, rowid ASC;
                """;

            command.Parameters.AddWithValue("$messageId", messageId);

            await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(MapProcessingResult(reader));
            }

            return results;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Oeffnet eine Verbindung und aktiviert wichtige SQLite-Optionen fuer den Lauf.
    /// Die Aktivierung von Foreign Keys ist wichtig, weil SQLite diese nicht global erzwungen
    /// einschaltet.
    /// </summary>
    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        SqliteConnection connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqliteCommand pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
        await pragmaCommand.ExecuteNonQueryAsync(cancellationToken);

        return connection;
    }

    /// <summary>
    /// Legt die Datenbankdatei und das benoetigte Schema direkt im Konstruktor an.
    /// Dadurch ist der Repository-Baustein fuer Entwickler sofort benutzbar, ohne dass sie
    /// einen separaten Initialisierungsschritt kennen oder vergessen muessen.
    /// </summary>
    private void EnsureSchemaCreated()
    {
        using SqliteConnection connection = new SqliteConnection(_connectionString);
        connection.Open();

        using SqliteCommand pragmaCommand = connection.CreateCommand();
        pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCommand.ExecuteNonQuery();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS schema_info
            (
                Name TEXT NOT NULL PRIMARY KEY,
                Value TEXT NOT NULL
            );

            INSERT INTO schema_info (Name, Value)
            VALUES ('SchemaVersion', '1')
            ON CONFLICT(Name) DO NOTHING;

            CREATE TABLE IF NOT EXISTS raw_messages
            (
                Id TEXT NOT NULL PRIMARY KEY,
                AccountId TEXT NOT NULL,
                ExternalMessageKey TEXT NOT NULL,
                SourceIdentifier TEXT NOT NULL,
                Fingerprint TEXT NOT NULL,
                RetrievedAtUtc TEXT NOT NULL,
                StoragePath TEXT NOT NULL,
                ApproximateSizeInBytes INTEGER NOT NULL,
                State INTEGER NOT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS IX_RawMessages_Account_ExternalKey
            ON raw_messages (AccountId, ExternalMessageKey);

            CREATE INDEX IF NOT EXISTS IX_RawMessages_Fingerprint
            ON raw_messages (Fingerprint);

            CREATE TABLE IF NOT EXISTS parsed_messages
            (
                Id TEXT NOT NULL PRIMARY KEY,
                AccountId TEXT NOT NULL,
                HeaderMessageId TEXT NOT NULL,
                Subject TEXT NOT NULL,
                FromJson TEXT NOT NULL,
                ToJson TEXT NOT NULL,
                CcJson TEXT NOT NULL,
                ReplyToJson TEXT NOT NULL,
                SentAtUtc TEXT NULL,
                ReceivedAtUtc TEXT NOT NULL,
                TextBody TEXT NOT NULL,
                HtmlBody TEXT NOT NULL,
                HeadersJson TEXT NOT NULL,
                AttachmentsJson TEXT NOT NULL,
                NormalizedText TEXT NOT NULL,
                TagsJson TEXT NOT NULL,
                State INTEGER NOT NULL,
                FOREIGN KEY (Id) REFERENCES raw_messages (Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS IX_ParsedMessages_ReceivedAtUtc
            ON parsed_messages (ReceivedAtUtc DESC);

            CREATE TABLE IF NOT EXISTS processing_results
            (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                MessageId TEXT NOT NULL,
                ProcessorName TEXT NOT NULL,
                Success INTEGER NOT NULL,
                StartedAtUtc TEXT NOT NULL,
                FinishedAtUtc TEXT NOT NULL,
                TagsJson TEXT NOT NULL,
                ExtractedDataJson TEXT NOT NULL,
                NotesJson TEXT NOT NULL,
                FOREIGN KEY (MessageId) REFERENCES raw_messages (Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS IX_ProcessingResults_MessageId_StartedAtUtc
            ON processing_results (MessageId, StartedAtUtc);
            """;
        command.ExecuteNonQuery();
    }

    private static RawMessage MapRawMessage(SqliteDataReader reader)
    {
        return new RawMessage
        {
            Id = reader.GetString(0),
            AccountId = reader.GetString(1),
            ExternalMessageKey = reader.GetString(2),
            SourceIdentifier = reader.GetString(3),
            Fingerprint = reader.GetString(4),
            RetrievedAtUtc = ParseIsoString(reader.GetString(5)),
            StoragePath = reader.GetString(6),
            ApproximateSizeInBytes = reader.GetInt64(7),
            State = (MessageProcessingState)reader.GetInt32(8)
        };
    }

    private static ParsedMessage MapParsedMessage(SqliteDataReader reader)
    {
        return new ParsedMessage
        {
            Id = reader.GetString(0),
            AccountId = reader.GetString(1),
            HeaderMessageId = reader.GetString(2),
            Subject = reader.GetString(3),
            From = DeserializeOrDefault<MailAddress?>(reader.GetString(4), null),
            To = DeserializeOrDefault(reader.GetString(5), new List<MailAddress>()),
            Cc = DeserializeOrDefault(reader.GetString(6), new List<MailAddress>()),
            ReplyTo = DeserializeOrDefault<MailAddress?>(reader.GetString(7), null),
            SentAtUtc = reader.IsDBNull(8) ? null : ParseIsoString(reader.GetString(8)),
            ReceivedAtUtc = ParseIsoString(reader.GetString(9)),
            TextBody = reader.GetString(10),
            HtmlBody = reader.GetString(11),
            Headers = DeserializeOrDefault(reader.GetString(12), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            Attachments = DeserializeOrDefault(reader.GetString(13), new List<AttachmentInfo>()),
            NormalizedText = reader.GetString(14),
            Tags = DeserializeOrDefault(reader.GetString(15), new List<string>()),
            State = (MessageProcessingState)reader.GetInt32(16)
        };
    }

    private static ProcessingResult MapProcessingResult(SqliteDataReader reader)
    {
        return new ProcessingResult
        {
            MessageId = reader.GetString(0),
            ProcessorName = reader.GetString(1),
            Success = reader.GetInt32(2) == 1,
            StartedAtUtc = ParseIsoString(reader.GetString(3)),
            FinishedAtUtc = ParseIsoString(reader.GetString(4)),
            Tags = DeserializeOrDefault(reader.GetString(5), new List<string>()),
            ExtractedData = DeserializeOrDefault(reader.GetString(6), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            Notes = DeserializeOrDefault(reader.GetString(7), new List<ProcessingNote>())
        };
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static T DeserializeOrDefault<T>(string json, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return defaultValue;
        }

        T? value = JsonSerializer.Deserialize<T>(json, JsonOptions);
        return value is null ? defaultValue : value;
    }

    private static string ToIsoString(DateTimeOffset value)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ParseIsoString(string value)
    {
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}
