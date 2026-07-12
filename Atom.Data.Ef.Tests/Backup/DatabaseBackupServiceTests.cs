using Genius.Atom.Data.Ef.Backup;
using Genius.Atom.Data.Ef.Tests.TestData;
using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.TestingUtil;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Genius.Atom.Data.Ef.Tests.Backup;

public sealed class DatabaseBackupServiceTests
{
    [Fact]
    public async Task CreateBackupAsync_GivenMoreBackupsThanRetention_KeepsOnlyNewestAndNamesByTimestamp()
    {
        // Arrange
        using var temp = new TempDatabase();
        temp.CreateViaMigrations();
        var dateTime = new FakeDateTime();
        var options = new DatabaseBackupOptions { Enabled = true, MaxBackups = 3 };
        var service = CreateService(temp, options, dateTime);

        // Act - create 5 backups, one day apart
        for (var i = 0; i < 5; i++)
        {
            var path = await service.CreateBackupAsync("test", TestContext.Current.CancellationToken);
            Assert.NotNull(path);
            dateTime.Advance(TimeSpan.FromDays(1));
        }

        // Assert - retention keeps only the newest 3, all timestamp-named
        var files = Directory.GetFiles(temp.BackupsDirectory, "*.bak");
        Assert.Equal(3, files.Length);
        Assert.All(files, file =>
            Assert.Matches(@"^TestItems\.db\.\d{8}-\d{6}\.bak$", Path.GetFileName(file)));
    }

    [Fact]
    public async Task CreateBackupAsync_ProducesReadableSqliteDatabaseWithData()
    {
        // Arrange
        using var temp = new TempDatabase();
        temp.CreateViaMigrations();
        await using (var context = new TestItemsDbContext(temp.Options))
        {
            await context.Items.AddAsync(new TestItem { Name = "backed-up", Description = "important" }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
        var service = CreateService(temp, new DatabaseBackupOptions(), new FakeDateTime());

        // Act
        var backupPath = await service.CreateBackupAsync("test", TestContext.Current.CancellationToken);

        // Assert - the backup can be opened as a standalone SQLite database and contains the data
        Assert.NotNull(backupPath);
        using var connection = new SqliteConnection($"Data Source={backupPath};Mode=ReadOnly;Pooling=False");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Items WHERE Name = 'backed-up'";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken), System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateBackupIfDueAsync_GivenRecentBackup_SkipsUntilIntervalElapsed()
    {
        // Arrange
        using var temp = new TempDatabase();
        temp.CreateViaMigrations();
        var dateTime = new FakeDateTime();
        var options = new DatabaseBackupOptions { Enabled = true, MaxBackups = 5, IntervalDays = 7 };
        var service = CreateService(temp, options, dateTime);
        await service.CreateBackupAsync("initial", TestContext.Current.CancellationToken);

        // Act / Assert - within the interval nothing happens
        var withinInterval = await service.CreateBackupIfDueAsync(TestContext.Current.CancellationToken);
        Assert.Null(withinInterval);

        // Act / Assert - once the interval elapses a new backup is created
        dateTime.Advance(TimeSpan.FromDays(8));
        var afterInterval = await service.CreateBackupIfDueAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(afterInterval);

        Assert.Equal(2, Directory.GetFiles(temp.BackupsDirectory, "*.bak").Length);
    }

    [Fact]
    public async Task CreateBackupAsync_GivenDisabled_DoesNothing()
    {
        // Arrange
        using var temp = new TempDatabase();
        temp.CreateViaMigrations();
        var service = CreateService(temp, new DatabaseBackupOptions { Enabled = false }, new FakeDateTime());

        // Act
        var path = await service.CreateBackupAsync("test", TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(path);
        Assert.False(Directory.Exists(temp.BackupsDirectory));
    }

    private static DatabaseBackupService<TestItemsDbContext> CreateService(
        TempDatabase temp, DatabaseBackupOptions options, IDateTime dateTime)
    {
        return new DatabaseBackupService<TestItemsDbContext>(
            temp.Options, options, dateTime, NullLogger<DatabaseBackupService<TestItemsDbContext>>.Instance);
    }
}
