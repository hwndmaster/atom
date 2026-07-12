using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef.Tests.TestData;

internal sealed class TempDatabase : IDisposable
{
    public TempDatabase()
    {
        Directory = System.IO.Directory.CreateTempSubdirectory("atom-dataef-tests").FullName;
        DbPath = Path.Combine(Directory, "TestItems.db");
        Options = new DbContextOptionsBuilder<TestItemsDbContext>()
            .UseSqlite($"Data Source={DbPath}")
            .Options;
    }

    public string Directory { get; }

    public string DbPath { get; }

    public DbContextOptions<TestItemsDbContext> Options { get; }

    public string BackupsDirectory => Path.Combine(Directory, "Backups");

    public void CreateViaMigrations()
    {
        using var context = new TestItemsDbContext(Options);
        context.Database.Migrate();
    }

    /// <summary>
    /// Simulates a database originally created with <c>EnsureCreated()</c> before the follow-up
    /// migration existed: the InitialCreate schema is present (with data), but there is no
    /// <c>__EFMigrationsHistory</c> table.
    /// </summary>
    public void CreateLegacySchemaWithoutHistory()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "Items" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Items" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL
            );
            INSERT INTO "Items" ("Name") VALUES ('pre-existing');
            """;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }
        catch (IOException)
        {
            // Best effort cleanup of the temp directory.
        }
    }
}
