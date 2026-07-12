using System.Data.Common;
using System.Globalization;
using Genius.Atom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Ef.Backup;

/// <summary>
/// Backs up a file-based SQLite database using SQLite's <c>VACUUM INTO</c> statement,
/// which produces a consistent snapshot while the database is in use. The backup is
/// issued through the plain ADO.NET connection, so no SQLite-specific package reference
/// is required; the registered <typeparamref name="TDbContext"/> must use the
/// <c>Microsoft.EntityFrameworkCore.Sqlite</c> provider.
/// </summary>
internal sealed class DatabaseBackupService<TDbContext> : IDatabaseBackupService
    where TDbContext : DbContext
{
    private const string BackupSubfolder = "Backups";
    private const string TimestampFormat = "yyyyMMdd-HHmmss";
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    private readonly DbContextOptions<TDbContext> _dbContextOptions;
    private readonly DatabaseBackupOptions _options;
    private readonly IDateTime _dateTime;
    private readonly ILogger<DatabaseBackupService<TDbContext>> _logger;

    public DatabaseBackupService(
        DbContextOptions<TDbContext> dbContextOptions,
        DatabaseBackupOptions options,
        IDateTime dateTime,
        ILogger<DatabaseBackupService<TDbContext>> logger)
    {
        _dbContextOptions = dbContextOptions.NotNull();
        _options = options.NotNull();
        _dateTime = dateTime.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<string?> CreateBackupAsync(string reason, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        await using var context = CreateDbContext();
        EnsureSqliteProvider(context);

        var connection = context.Database.GetDbConnection();
        var dbPath = GetDatabaseFilePath(connection);
        if (dbPath is null)
        {
            _logger.LogDebug("Skipping database backup ({Reason}): database is not file-based.", reason);
            return null;
        }

        if (!File.Exists(dbPath))
        {
            _logger.LogInformation("Skipping database backup ({Reason}): database file does not exist yet at {DbPath}.", reason, dbPath);
            return null;
        }

        var backupDir = Path.Combine(Path.GetDirectoryName(dbPath)!, BackupSubfolder);
        Directory.CreateDirectory(backupDir);

        var dbFileName = Path.GetFileName(dbPath);
        var timestamp = _dateTime.NowUtc.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        var backupPath = Path.Combine(backupDir, $"{dbFileName}.{timestamp}.bak");

        // VACUUM INTO refuses to overwrite an existing file.
        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }

        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using (var command = connection.CreateCommand())
        {
            // The target of VACUUM INTO cannot be parameterized; the path is derived from the
            // connection's data source (not user input) and single quotes are escaped.
#pragma warning disable CA2100, S2077
            command.CommandText = $"VACUUM INTO '{backupPath.Replace("'", "''", StringComparison.Ordinal)}'";
#pragma warning restore CA2100, S2077
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Database backup created ({Reason}): {BackupPath}.", reason, backupPath);

        PruneOldBackups(backupDir, $"{dbFileName}.*.bak");

        return backupPath;
    }

    public async Task<string?> CreateBackupIfDueAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        string? dbPath;
        await using (var context = CreateDbContext())
        {
            EnsureSqliteProvider(context);
            dbPath = GetDatabaseFilePath(context.Database.GetDbConnection());
        }

        if (dbPath is null)
        {
            return null;
        }

        var backupDir = Path.Combine(Path.GetDirectoryName(dbPath)!, BackupSubfolder);
        var dbFileName = Path.GetFileName(dbPath);

        var lastBackupUtc = GetLatestBackupTimestampUtc(backupDir, dbFileName);
        if (lastBackupUtc is not null)
        {
            var age = _dateTime.NowUtc - lastBackupUtc.Value;
            if (age < TimeSpan.FromDays(_options.IntervalDays))
            {
                _logger.LogDebug("Skipping scheduled backup: most recent backup is {Age} old (interval {IntervalDays}d).", age, _options.IntervalDays);
                return null;
            }
        }

        return await CreateBackupAsync("scheduled", cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the timestamp encoded in the most recent backup file name, or <c>null</c> when no backup exists.
    /// The timestamp is read from the file name (written using the injectable clock) rather than the file's
    /// modification time, keeping the schedule consistent with <see cref="IDateTime"/>.
    /// </summary>
    private static DateTime? GetLatestBackupTimestampUtc(string backupDir, string dbFileName)
    {
        if (!Directory.Exists(backupDir))
        {
            return null;
        }

        var prefix = $"{dbFileName}.";
        DateTime? latest = null;
        foreach (var path in Directory.GetFiles(backupDir, $"{dbFileName}.*.bak"))
        {
            var fileName = Path.GetFileName(path);
            var stamp = fileName[prefix.Length..^".bak".Length];
            if (DateTime.TryParseExact(stamp, TimestampFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
                && (latest is null || parsed > latest))
            {
                latest = parsed;
            }
        }

        return latest;
    }

    private TDbContext CreateDbContext()
    {
        return (TDbContext?)Activator.CreateInstance(typeof(TDbContext), _dbContextOptions)
            ?? throw new InvalidOperationException($"Failed to create an instance of {typeof(TDbContext).Name}.");
    }

    private static void EnsureSqliteProvider(TDbContext context)
    {
        var providerName = context.Database.ProviderName;
        if (!string.Equals(providerName, SqliteProviderName, StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                $"Database backups are only supported for the SQLite provider, but '{typeof(TDbContext).Name}' uses '{providerName}'.");
        }
    }

    private static string? GetDatabaseFilePath(DbConnection connection)
    {
        var dataSource = connection.DataSource;

        // In-memory databases (commonly used by integration tests) have no backing file.
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return Path.GetFullPath(dataSource);
    }

    private void PruneOldBackups(string backupDir, string searchPattern)
    {
        var staleBackups = Directory.GetFiles(backupDir, searchPattern)
            .OrderByDescending(path => path, StringComparer.Ordinal)
            .Skip(Math.Max(_options.MaxBackups, 0))
            .ToList();

        foreach (var staleBackup in staleBackups)
        {
            try
            {
                File.Delete(staleBackup);
                _logger.LogInformation("Pruned old database backup: {BackupPath}.", staleBackup);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "Failed to prune old database backup: {BackupPath}.", staleBackup);
            }
        }
    }
}
