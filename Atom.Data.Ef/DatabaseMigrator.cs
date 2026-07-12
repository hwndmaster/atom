using Genius.Atom.Data.Ef.Backup;
using Genius.Atom.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Ef;

internal sealed class DatabaseMigrator<TDbContext> : IDatabaseMigrator
    where TDbContext : DbContext
{
    private readonly TDbContext _context;
    private readonly IDatabaseBackupService _backupService;
    private readonly ILogger<DatabaseMigrator<TDbContext>> _logger;

    public DatabaseMigrator(
        TDbContext context,
        IDatabaseBackupService backupService,
        ILogger<DatabaseMigrator<TDbContext>> logger)
    {
        _context = context.NotNull();
        _backupService = backupService.NotNull();
        _logger = logger.NotNull();
    }

    public async Task MigrateWithBackupAsync(CancellationToken cancellationToken = default)
    {
        // Databases created with EnsureCreated() lack the EF migrations history table, while their
        // schema corresponds to the initial migration. Baseline them by recording only the initial
        // migration as applied: EF will not re-run it, and any later migration is applied normally
        // to bring the schema up to date.
        var databaseCreator = (RelationalDatabaseCreator)_context.GetService<IRelationalDatabaseCreator>();
        var hasSchemaWithoutHistory = await databaseCreator.ExistsAsync(cancellationToken).ConfigureAwait(false)
            && await databaseCreator.HasTablesAsync(cancellationToken).ConfigureAwait(false)
            && !(await _context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any();
        if (hasSchemaWithoutHistory)
        {
            await BaselineExistingDatabaseAsync(cancellationToken).ConfigureAwait(false);
        }

        var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();
        if (pendingMigrations.Count == 0)
        {
            return;
        }

        // Back up before mutating the schema (no-op for a brand-new database that does not exist yet).
        await _backupService.CreateBackupAsync($"pre-migration ({pendingMigrations.Count} pending)", cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Applying {Count} pending database migration(s): {Migrations}.",
            pendingMigrations.Count, string.Join(", ", pendingMigrations));
        await _context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task BaselineExistingDatabaseAsync(CancellationToken cancellationToken)
    {
        var initialMigrationId = _context.Database.GetMigrations().FirstOrDefault();
        if (initialMigrationId is null)
        {
            return;
        }

        _logger.LogInformation("Baselining existing database without migrations history: recording {Migration} as applied.",
            initialMigrationId);

        var historyRepository = _context.GetService<IHistoryRepository>();
        var productVersion = ProductInfo.GetVersion();

        await _context.Database.ExecuteSqlRawAsync(historyRepository.GetCreateIfNotExistsScript(), cancellationToken).ConfigureAwait(false);
        var insertScript = historyRepository.GetInsertScript(new HistoryRow(initialMigrationId, productVersion));
        await _context.Database.ExecuteSqlRawAsync(insertScript, cancellationToken).ConfigureAwait(false);
    }
}
