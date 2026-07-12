using Genius.Atom.Data.Ef.Backup;
using Genius.Atom.Data.Ef.Tests.TestData;
using Genius.Atom.Infrastructure.TestingUtil;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Genius.Atom.Data.Ef.Tests;

public sealed class DatabaseMigratorTests
{
    [Fact]
    public async Task MigrateWithBackupAsync_GivenFreshDatabase_AppliesAllMigrations()
    {
        // Arrange
        using var temp = new TempDatabase();

        // Act
        await RunMigratorAsync(temp);

        // Assert - both migrations applied, the full schema (incl. Description) is usable
        await using var context = new TestItemsDbContext(temp.Options);
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, applied.Count());
        await context.Items.AddAsync(new TestItem { Name = "fresh", Description = "works" }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MigrateWithBackupAsync_GivenLegacyDatabaseWithoutMigrationsHistory_BaselinesInitialAndAppliesTheRest()
    {
        // Arrange - a database created with EnsureCreated() before the follow-up migration existed:
        // InitialCreate schema, existing data, no __EFMigrationsHistory table.
        using var temp = new TempDatabase();
        temp.CreateLegacySchemaWithoutHistory();

        // Act
        await RunMigratorAsync(temp);

        // Assert - InitialCreate was baselined (recorded, not executed) and AddItemDescription applied
        await using var context = new TestItemsDbContext(temp.Options);
        var applied = (await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken)).ToList();
        Assert.Equal(2, applied.Count);
        Assert.Contains("20240101000000_InitialCreate", applied);
        Assert.Contains("20240201000000_AddItemDescription", applied);

        // Existing data survived and the new column is present
        var item = await context.Items.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("pre-existing", item.Name);
        Assert.Null(item.Description);

        // A pre-migration backup was taken before the schema change
        Assert.Single(Directory.GetFiles(temp.BackupsDirectory, "*.bak"));
    }

    [Fact]
    public async Task MigrateWithBackupAsync_GivenUpToDateDatabase_DoesNothing()
    {
        // Arrange
        using var temp = new TempDatabase();
        await RunMigratorAsync(temp);

        // Act - a second run finds no pending migrations
        await RunMigratorAsync(temp);

        // Assert - still exactly two applied migrations and no backup was taken
        await using var context = new TestItemsDbContext(temp.Options);
        var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, applied.Count());
        Assert.False(Directory.Exists(temp.BackupsDirectory));
    }

    private static async Task RunMigratorAsync(TempDatabase temp)
    {
        await using var context = new TestItemsDbContext(temp.Options);
        var backupService = new DatabaseBackupService<TestItemsDbContext>(
            temp.Options, new DatabaseBackupOptions(), new FakeDateTime(),
            NullLogger<DatabaseBackupService<TestItemsDbContext>>.Instance);
        var migrator = new DatabaseMigrator<TestItemsDbContext>(
            context, backupService, NullLogger<DatabaseMigrator<TestItemsDbContext>>.Instance);
        await migrator.MigrateWithBackupAsync(TestContext.Current.CancellationToken);
    }
}
