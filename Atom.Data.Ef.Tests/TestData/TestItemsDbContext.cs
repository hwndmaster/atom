using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef.Tests.TestData;

public sealed class TestItemsDbContext : DbContext
{
    public TestItemsDbContext(DbContextOptions<TestItemsDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestItem> Items => Set<TestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestItem>(entity =>
        {
            entity.ToTable("Items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
        });
    }
}

public sealed class TestItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
