using Genius.Atom.Data.Ef.Tests.TestData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Genius.Atom.Data.Ef.Tests;

public sealed class ReferenceModelBuilderExtensionsTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ReferenceItemsDbContext> _options;

    public ReferenceModelBuilderExtensionsTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ReferenceItemsDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ConfigureReferenceId_GivenInt32Key_LetsTheDatabaseGenerateTheIdentifier()
    {
        // Arrange
        await using var context = CreateContext();

        // Act - the identifier is left at its default, i.e. at the registered sentinel
        var item = new IntItem { Name = "generated" };
        await context.IntItems.AddAsync(item, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(0, item.Id.Id);
        await using var readContext = CreateContext();
        var reloaded = await readContext.IntItems.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(item.Id, reloaded.Id);
        Assert.Equal("generated", reloaded.Name);
    }

    [Fact]
    public async Task ConfigureReferenceId_GivenInt32KeyAndClientGeneration_KeepsTheAssignedIdentifier()
    {
        // Arrange
        await using var context = CreateContext();

        // Act
        await context.ClientIntItems.AddAsync(
            new ClientIntItem { Id = new ClientIntItemRef(4242), Name = "assigned" },
            TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await using var readContext = CreateContext();
        var reloaded = await readContext.ClientIntItems.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new ClientIntItemRef(4242), reloaded.Id);
    }

    [Fact]
    public async Task ConfigureReferenceId_GivenGuidKey_KeepsTheAssignedIdentifier()
    {
        // Arrange
        var id = new GuidItemRef(Guid.NewGuid());
        await using var context = CreateContext();

        // Act
        await context.GuidItems.AddAsync(new GuidItem { Id = id, Name = "assigned" },
            TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await using var readContext = CreateContext();
        var reloaded = await readContext.GuidItems.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(id, reloaded.Id);
    }

    [Theory]
    [InlineData(typeof(IntItem), typeof(int), ValueGenerated.OnAdd)]
    [InlineData(typeof(ClientIntItem), typeof(int), ValueGenerated.Never)]
    [InlineData(typeof(GuidItem), typeof(Guid), ValueGenerated.Never)]
    public void ConfigureReferenceId_MapsTheIdentifierToItsUnderlyingKeyType(
        Type entityType, Type expectedProviderType, ValueGenerated expectedValueGenerated)
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var property = context.Model.FindEntityType(entityType)!.FindProperty(nameof(IntItem.Id))!;

        // Assert
        Assert.Equal(expectedProviderType, property.GetValueConverter()!.ProviderClrType);
        Assert.Equal(expectedValueGenerated, property.ValueGenerated);
    }

    [Fact]
    public void ConfigureReferenceId_GivenDatabaseGeneration_RegistersTheDefaultKeyAsSentinel()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var property = context.Model.FindEntityType(typeof(IntItem))!.FindProperty(nameof(IntItem.Id))!;

        // Assert - EF Core needs the sentinel to recognize an unset identifier as requiring generation
        Assert.Equal(new IntItemRef(0), property.Sentinel);
    }

    [Fact]
    public async Task ConfigureReferenceFk_GivenInt32Key_StoresTheUnderlyingKeyAndReadsBackTheReference()
    {
        // Arrange
        await using var context = CreateContext();
        var parent = new IntItem { Name = "parent" };
        await context.IntItems.AddAsync(parent, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await context.IntChildren.AddAsync(new IntChild { ParentId = parent.Id }, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert - the column holds the raw key, ...
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT ParentId FROM IntChildren";
        Assert.Equal((long)parent.Id.Id, await command.ExecuteScalarAsync(TestContext.Current.CancellationToken));

        // ... and querying by the strongly-typed reference translates through the converter
        await using var readContext = CreateContext();
        var reloaded = await readContext.IntChildren
            .SingleAsync(x => x.ParentId == parent.Id, TestContext.Current.CancellationToken);
        Assert.Equal(parent.Id, reloaded.ParentId);
    }

    [Fact]
    public async Task ConfigureReferenceFk_GivenGuidKey_StoresTheUnderlyingKeyAndReadsBackTheReference()
    {
        // Arrange
        await using var context = CreateContext();
        var parent = new GuidItem { Id = new GuidItemRef(Guid.NewGuid()), Name = "parent" };
        await context.GuidItems.AddAsync(parent, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await context.GuidChildren.AddAsync(
            new GuidChild { Id = new GuidChildRef(Guid.NewGuid()), ParentId = parent.Id },
            TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT ParentId FROM GuidChildren";
        var storedKey = (string)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
        Assert.Equal(parent.Id.Id, Guid.Parse(storedKey));

        await using var readContext = CreateContext();
        var reloaded = await readContext.GuidChildren
            .SingleAsync(x => x.ParentId == parent.Id, TestContext.Current.CancellationToken);
        Assert.Equal(parent.Id, reloaded.ParentId);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private ReferenceItemsDbContext CreateContext() => new(_options);
}
