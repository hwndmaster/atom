using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef.Tests.TestData;

/// <summary>
/// A context exercising every flavour of <see cref="ReferenceModelBuilderExtensions"/>:
/// database- and client-generated <see cref="int"/> keys, client-generated <see cref="Guid"/> keys,
/// and foreign keys of both key types.
/// </summary>
internal sealed class ReferenceItemsDbContext : DbContext
{
    public ReferenceItemsDbContext(DbContextOptions<ReferenceItemsDbContext> options)
        : base(options)
    {
    }

    public DbSet<IntItem> IntItems => Set<IntItem>();
    public DbSet<IntChild> IntChildren => Set<IntChild>();
    public DbSet<ClientIntItem> ClientIntItems => Set<ClientIntItem>();
    public DbSet<GuidItem> GuidItems => Set<GuidItem>();
    public DbSet<GuidChild> GuidChildren => Set<GuidChild>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureReferenceId<IntItem, IntItemRef>(id => new IntItemRef(id));
        modelBuilder.ConfigureReferenceId<IntChild, IntChildRef>(id => new IntChildRef(id));
        modelBuilder.ConfigureReferenceId<ClientIntItem, ClientIntItemRef>(id => new ClientIntItemRef(id),
            ReferenceIdGeneration.Client);
        modelBuilder.ConfigureReferenceId<GuidItem, GuidItemRef>(id => new GuidItemRef(id));
        modelBuilder.ConfigureReferenceId<GuidChild, GuidChildRef>(id => new GuidChildRef(id));

        modelBuilder.ConfigureReferenceFk<IntChild, IntItemRef>(nameof(IntChild.ParentId), id => new IntItemRef(id));
        modelBuilder.ConfigureReferenceFk<GuidChild, GuidItemRef>(nameof(GuidChild.ParentId), id => new GuidItemRef(id));
    }
}

internal sealed record IntItemRef(int Id) : IReference<int, IntItemRef>
{
    public static IntItemRef Create(int id) => new(id);
}

internal sealed record IntChildRef(int Id) : IReference<int, IntChildRef>
{
    public static IntChildRef Create(int id) => new(id);
}

internal sealed record ClientIntItemRef(int Id) : IReference<int, ClientIntItemRef>
{
    public static ClientIntItemRef Create(int id) => new(id);
}

internal sealed record GuidItemRef(Guid Id) : IReference<Guid, GuidItemRef>
{
    public static GuidItemRef Create(Guid id) => new(id);
}

internal sealed record GuidChildRef(Guid Id) : IReference<Guid, GuidChildRef>
{
    public static GuidChildRef Create(Guid id) => new(id);
}

internal sealed record IntItem : EntityBase<int, IntItemRef>
{
    public required string Name { get; init; }
}

internal sealed record IntChild : EntityBase<int, IntChildRef>
{
    public required IntItemRef ParentId { get; init; }
}

internal sealed record ClientIntItem : EntityBase<int, ClientIntItemRef>
{
    public required string Name { get; init; }
}

internal sealed record GuidItem : EntityBase<Guid, GuidItemRef>
{
    public required string Name { get; init; }
}

internal sealed record GuidChild : EntityBase<Guid, GuidChildRef>
{
    public required GuidItemRef ParentId { get; init; }
}
