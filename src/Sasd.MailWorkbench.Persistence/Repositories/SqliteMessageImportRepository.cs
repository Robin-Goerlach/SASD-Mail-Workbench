using System.Globalization;
using Microsoft.Data.Sqlite;
using Sasd.MailWorkbench.Application.Abstractions;
using Sasd.MailWorkbench.Domain.Enums;
using Sasd.MailWorkbench.Domain.Models;
using Sasd.MailWorkbench.Domain.Rules;
using Sasd.MailWorkbench.Domain.ValueObjects;
using Sasd.MailWorkbench.Persistence.Migrations;

namespace Sasd.MailWorkbench.Persistence.Repositories;

/// <summary>
/// SQLite-Implementierung des Importkatalogs. Jede öffentliche Operation öffnet
/// eine eigene Verbindung; SQLite koordiniert parallele Leser und Schreiber.
/// </summary>
public sealed class SqliteMessageImportRepository : IMessageImportRepository
{
    private readonly string _connectionString;
    private readonly SqliteMigrationRunner _migrationRunner;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    /// <summary>
    /// Erstellt ein Repository für die angegebene SQLite-Datei.
    /// </summary>
    public SqliteMessageImportRepository(string databaseFilePath)
    {
        string fullPath = Path.GetFullPath(databaseFilePath ?? throw new ArgumentNullException(nameof(databaseFilePath)));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        }.ToString();
        _migrationRunner = new SqliteMigrationRunner(_connectionString);
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_initialized)
            {
                await _migrationRunner.MigrateAsync(cancellationToken).ConfigureAwait(false);
                _initialized = true;
            }
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<StoredRawMessage?> FindBySourceKeyAsync(
        string accountId,
        string sourceKey,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.id, m.account_id, m.sha256, m.byte_length, m.relative_path, m.imported_at_utc
            FROM source_observations o
            JOIN raw_messages m ON m.id = o.message_id
            WHERE o.account_id=$accountId AND o.source_key=$sourceKey;
            """;
        command.Parameters.AddWithValue("$accountId", accountId);
        command.Parameters.AddWithValue("$sourceKey", sourceKey);
        return await ReadSingleMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<StoredRawMessage?> FindByFingerprintAsync(
        string accountId,
        MessageFingerprint fingerprint,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, account_id, sha256, byte_length, relative_path, imported_at_utc
            FROM raw_messages
            WHERE account_id=$accountId AND sha256=$sha256 AND byte_length=$byteLength;
            """;
        command.Parameters.AddWithValue("$accountId", accountId);
        command.Parameters.AddWithValue("$sha256", fingerprint.Sha256);
        command.Parameters.AddWithValue("$byteLength", fingerprint.LengthInBytes);
        return await ReadSingleMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CreateAttemptAsync(ImportAttempt attempt, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO import_attempts(
                id, account_id, source_key, state, staging_relative_path,
                target_relative_path, sha256, byte_length, existing_message_id,
                started_at_utc, updated_at_utc, completed_at_utc, retry_count,
                error_code, error_message)
            VALUES(
                $id, $accountId, $sourceKey, $state, $stagingPath,
                $targetPath, $sha256, $byteLength, $existingMessageId,
                $startedAt, $updatedAt, $completedAt, $retryCount,
                $errorCode, $errorMessage);
            """;
        AddAttemptParameters(command, attempt);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAttemptAsync(ImportAttempt attempt, CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        ImportAttempt? current = await GetAttemptAsync(connection, attempt.Id, cancellationToken).ConfigureAwait(false);
        if (current is null)
        {
            throw new InvalidOperationException($"Importversuch {attempt.Id} existiert nicht.");
        }

        if (current.State != attempt.State && !ImportStateRules.CanTransition(current.State, attempt.State))
        {
            throw new InvalidOperationException($"Unzulässiger Zustandswechsel {current.State} -> {attempt.State}.");
        }

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            UPDATE import_attempts SET
                state=$state,
                staging_relative_path=$stagingPath,
                target_relative_path=$targetPath,
                sha256=$sha256,
                byte_length=$byteLength,
                existing_message_id=$existingMessageId,
                updated_at_utc=$updatedAt,
                completed_at_utc=$completedAt,
                retry_count=$retryCount,
                error_code=$errorCode,
                error_message=$errorMessage
            WHERE id=$id;
            """;
        AddAttemptParameters(command, attempt);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<CatalogCompletionResult> CompleteNewMessageAsync(
        ImportAttempt attempt,
        StoredRawMessage candidateMessage,
        SourceObservation observation,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteTransaction transaction = (SqliteTransaction)await connection
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using SqliteCommand insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO raw_messages(id, account_id, sha256, byte_length, relative_path, imported_at_utc)
                VALUES($id, $accountId, $sha256, $byteLength, $relativePath, $importedAt)
                ON CONFLICT(account_id, sha256, byte_length) DO NOTHING;
                """;
            AddMessageParameters(insert, candidateMessage);
            int rows = await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            StoredRawMessage canonical = await FindByFingerprintAsync(
                connection,
                candidateMessage.AccountId,
                candidateMessage.Fingerprint,
                transaction,
                cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Kanonische Nachricht konnte nach INSERT nicht gelesen werden.");

            SourceObservation effectiveObservation = observation with { MessageId = canonical.Id };
            await UpsertObservationAsync(connection, effectiveObservation, transaction, cancellationToken)
                .ConfigureAwait(false);

            ImportAttemptState finalState = rows == 1
                ? ImportAttemptState.Completed
                : ImportAttemptState.DuplicateDiscarded;
            await UpdateAttemptInTransactionAsync(connection, attempt with
            {
                State = finalState,
                ExistingMessageId = rows == 1 ? null : canonical.Id,
                CompletedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                ErrorCode = null,
                ErrorMessage = null
            }, transaction, cancellationToken).ConfigureAwait(false);

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new CatalogCompletionResult(rows == 1, canonical);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CompleteDuplicateAsync(
        ImportAttempt attempt,
        StoredRawMessage existingMessage,
        SourceObservation observation,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteTransaction transaction = (SqliteTransaction)await connection
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await UpsertObservationAsync(
                connection,
                observation with { MessageId = existingMessage.Id },
                transaction,
                cancellationToken).ConfigureAwait(false);
            await UpdateAttemptInTransactionAsync(connection, attempt with
            {
                State = ImportAttemptState.DuplicateDiscarded,
                ExistingMessageId = existingMessage.Id,
                CompletedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                ErrorCode = null,
                ErrorMessage = null
            }, transaction, cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ImportAttempt>> ListRecoverableAttemptsAsync(
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, account_id, source_key, state, staging_relative_path,
                   target_relative_path, sha256, byte_length, existing_message_id,
                   started_at_utc, updated_at_utc, completed_at_utc, retry_count,
                   error_code, error_message
            FROM import_attempts
            WHERE state IN ($downloading, $staged, $committing)
            ORDER BY started_at_utc;
            """;
        command.Parameters.AddWithValue("$downloading", (int)ImportAttemptState.Downloading);
        command.Parameters.AddWithValue("$staged", (int)ImportAttemptState.Staged);
        command.Parameters.AddWithValue("$committing", (int)ImportAttemptState.Committing);

        List<ImportAttempt> attempts = [];
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            attempts.Add(ReadAttempt(reader));
        }
        return attempts;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoredRawMessage>> ListMessagesAsync(CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, account_id, sha256, byte_length, relative_path, imported_at_utc
            FROM raw_messages ORDER BY imported_at_utc, id;
            """;
        List<StoredRawMessage> messages = [];
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            messages.Add(ReadMessage(reader));
        }
        return messages;
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);
        SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using SqliteCommand pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;";
        await pragma.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    private static async Task<StoredRawMessage?> ReadSingleMessageAsync(
        SqliteCommand command,
        CancellationToken cancellationToken)
    {
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadMessage(reader)
            : null;
    }

    private static StoredRawMessage ReadMessage(SqliteDataReader reader) => new(
        reader.GetString(0),
        reader.GetString(1),
        new MessageFingerprint(reader.GetString(2), reader.GetInt64(3)),
        reader.GetString(4),
        DateTimeOffset.Parse(reader.GetString(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    private static ImportAttempt ReadAttempt(SqliteDataReader reader)
    {
        MessageFingerprint? fingerprint = reader.IsDBNull(6)
            ? null
            : new MessageFingerprint(reader.GetString(6), reader.GetInt64(7));
        return new ImportAttempt
        {
            Id = reader.GetString(0),
            AccountId = reader.GetString(1),
            SourceKey = reader.GetString(2),
            State = (ImportAttemptState)reader.GetInt32(3),
            StagingRelativePath = reader.IsDBNull(4) ? null : reader.GetString(4),
            TargetRelativePath = reader.IsDBNull(5) ? null : reader.GetString(5),
            Fingerprint = fingerprint,
            ExistingMessageId = reader.IsDBNull(8) ? null : reader.GetString(8),
            StartedAtUtc = ParseDate(reader.GetString(9)),
            UpdatedAtUtc = ParseDate(reader.GetString(10)),
            CompletedAtUtc = reader.IsDBNull(11) ? null : ParseDate(reader.GetString(11)),
            RetryCount = reader.GetInt32(12),
            ErrorCode = reader.IsDBNull(13) ? null : reader.GetString(13),
            ErrorMessage = reader.IsDBNull(14) ? null : reader.GetString(14)
        };
    }

    private static async Task<ImportAttempt?> GetAttemptAsync(
        SqliteConnection connection,
        string id,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, account_id, source_key, state, staging_relative_path,
                   target_relative_path, sha256, byte_length, existing_message_id,
                   started_at_utc, updated_at_utc, completed_at_utc, retry_count,
                   error_code, error_message
            FROM import_attempts WHERE id=$id;
            """;
        command.Parameters.AddWithValue("$id", id);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        return await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
            ? ReadAttempt(reader)
            : null;
    }

    private static async Task<StoredRawMessage?> FindByFingerprintAsync(
        SqliteConnection connection,
        string accountId,
        MessageFingerprint fingerprint,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT id, account_id, sha256, byte_length, relative_path, imported_at_utc
            FROM raw_messages
            WHERE account_id=$accountId AND sha256=$sha256 AND byte_length=$byteLength;
            """;
        command.Parameters.AddWithValue("$accountId", accountId);
        command.Parameters.AddWithValue("$sha256", fingerprint.Sha256);
        command.Parameters.AddWithValue("$byteLength", fingerprint.LengthInBytes);
        return await ReadSingleMessageAsync(command, cancellationToken).ConfigureAwait(false);
    }

    private static async Task UpsertObservationAsync(
        SqliteConnection connection,
        SourceObservation observation,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO source_observations(
                account_id, source_key, message_id, first_seen_at_utc, last_seen_at_utc)
            VALUES($accountId, $sourceKey, $messageId, $firstSeen, $lastSeen)
            ON CONFLICT(account_id, source_key) DO UPDATE SET
                message_id=excluded.message_id,
                last_seen_at_utc=excluded.last_seen_at_utc;
            """;
        command.Parameters.AddWithValue("$accountId", observation.AccountId);
        command.Parameters.AddWithValue("$sourceKey", observation.SourceKey);
        command.Parameters.AddWithValue("$messageId", observation.MessageId);
        command.Parameters.AddWithValue("$firstSeen", observation.FirstSeenAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$lastSeen", observation.LastSeenAtUtc.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task UpdateAttemptInTransactionAsync(
        SqliteConnection connection,
        ImportAttempt attempt,
        SqliteTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE import_attempts SET
                state=$state, staging_relative_path=$stagingPath,
                target_relative_path=$targetPath, sha256=$sha256,
                byte_length=$byteLength, existing_message_id=$existingMessageId,
                updated_at_utc=$updatedAt, completed_at_utc=$completedAt,
                retry_count=$retryCount, error_code=$errorCode,
                error_message=$errorMessage
            WHERE id=$id;
            """;
        AddAttemptParameters(command, attempt);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddMessageParameters(SqliteCommand command, StoredRawMessage message)
    {
        command.Parameters.AddWithValue("$id", message.Id);
        command.Parameters.AddWithValue("$accountId", message.AccountId);
        command.Parameters.AddWithValue("$sha256", message.Fingerprint.Sha256);
        command.Parameters.AddWithValue("$byteLength", message.Fingerprint.LengthInBytes);
        command.Parameters.AddWithValue("$relativePath", message.RelativePath);
        command.Parameters.AddWithValue("$importedAt", message.ImportedAtUtc.ToString("O"));
    }

    private static void AddAttemptParameters(SqliteCommand command, ImportAttempt attempt)
    {
        command.Parameters.AddWithValue("$id", attempt.Id);
        command.Parameters.AddWithValue("$accountId", attempt.AccountId);
        command.Parameters.AddWithValue("$sourceKey", attempt.SourceKey);
        command.Parameters.AddWithValue("$state", (int)attempt.State);
        command.Parameters.AddWithValue("$stagingPath", DbValue(attempt.StagingRelativePath));
        command.Parameters.AddWithValue("$targetPath", DbValue(attempt.TargetRelativePath));
        command.Parameters.AddWithValue("$sha256", DbValue(attempt.Fingerprint?.Sha256));
        command.Parameters.AddWithValue("$byteLength", attempt.Fingerprint is null ? DBNull.Value : attempt.Fingerprint.LengthInBytes);
        command.Parameters.AddWithValue("$existingMessageId", DbValue(attempt.ExistingMessageId));
        command.Parameters.AddWithValue("$startedAt", attempt.StartedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", attempt.UpdatedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$completedAt", attempt.CompletedAtUtc is null ? DBNull.Value : attempt.CompletedAtUtc.Value.ToString("O"));
        command.Parameters.AddWithValue("$retryCount", attempt.RetryCount);
        command.Parameters.AddWithValue("$errorCode", DbValue(attempt.ErrorCode));
        command.Parameters.AddWithValue("$errorMessage", DbValue(attempt.ErrorMessage));
    }

    private static object DbValue(string? value) => value is null ? DBNull.Value : value;

    private static DateTimeOffset ParseDate(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
