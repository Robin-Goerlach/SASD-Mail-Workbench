using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Sasd.MailWorkbench.Persistence.Migrations;

/// <summary>
/// Führt eingebettete, unveränderliche SQL-Migrationen in Versionsreihenfolge aus.
/// </summary>
public sealed class SqliteMigrationRunner
{
    private readonly string _connectionString;

    /// <summary>
    /// Erstellt einen Runner für die angegebene SQLite-Verbindung.
    /// </summary>
    public SqliteMigrationRunner(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Wendet alle noch nicht ausgeführten eingebetteten Migrationen transaktional an.
    /// </summary>
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await ConfigureConnectionAsync(connection, cancellationToken).ConfigureAwait(false);
        await EnsureMigrationTableAsync(connection, cancellationToken).ConfigureAwait(false);

        foreach (MigrationResource migration in LoadMigrations())
        {
            string? existingChecksum = await GetAppliedChecksumAsync(
                connection,
                migration.Version,
                cancellationToken).ConfigureAwait(false);

            if (existingChecksum is not null)
            {
                if (!string.Equals(existingChecksum, migration.Checksum, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Migration {migration.Version} wurde nach ihrer Anwendung verändert.");
                }
                continue;
            }

            await using SqliteTransaction transaction = (SqliteTransaction)await connection
                .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await using SqliteCommand migrationCommand = connection.CreateCommand();
                migrationCommand.Transaction = transaction;
                migrationCommand.CommandText = migration.Sql;
                await migrationCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                await using SqliteCommand recordCommand = connection.CreateCommand();
                recordCommand.Transaction = transaction;
                recordCommand.CommandText = """
                    INSERT INTO schema_migrations(version, name, checksum, applied_at_utc)
                    VALUES ($version, $name, $checksum, $appliedAtUtc);
                    """;
                recordCommand.Parameters.AddWithValue("$version", migration.Version);
                recordCommand.Parameters.AddWithValue("$name", migration.Name);
                recordCommand.Parameters.AddWithValue("$checksum", migration.Checksum);
                recordCommand.Parameters.AddWithValue("$appliedAtUtc", DateTimeOffset.UtcNow.ToString("O"));
                await recordCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                throw;
            }
        }
    }

    private static async Task ConfigureConnectionAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000; PRAGMA journal_mode=WAL;";
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureMigrationTableAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                checksum TEXT NOT NULL,
                applied_at_utc TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string?> GetAppliedChecksumAsync(
        SqliteConnection connection,
        int version,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT checksum FROM schema_migrations WHERE version=$version;";
        command.Parameters.AddWithValue("$version", version);
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result as string;
    }

    private static IReadOnlyList<MigrationResource> LoadMigrations()
    {
        Assembly assembly = typeof(SqliteMigrationRunner).Assembly;
        return assembly.GetManifestResourceNames()
            .Where(name => name.Contains(".Migrations.", StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(name => ReadMigration(assembly, name))
            .OrderBy(migration => migration.Version)
            .ToArray();
    }

    private static MigrationResource ReadMigration(Assembly assembly, string resourceName)
    {
        string fileName = resourceName.Split(".Migrations.", StringSplitOptions.None)[1];
        int separator = fileName.IndexOf('_');
        if (separator <= 0 || !int.TryParse(fileName[..separator], out int version))
        {
            throw new InvalidOperationException($"Ungültiger Migrationsname: {fileName}");
        }

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Migration nicht lesbar: {resourceName}");
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string sql = reader.ReadToEnd();
        string checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
        return new MigrationResource(version, fileName, checksum, sql);
    }

    private sealed record MigrationResource(int Version, string Name, string Checksum, string Sql);
}
