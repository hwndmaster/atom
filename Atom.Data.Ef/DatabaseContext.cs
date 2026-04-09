using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Genius.Atom.Data.Ef;

internal sealed class DatabaseContext<TDbContext> : IDatabaseContext
    where TDbContext : DbContext
{
    private readonly DbContextOptions<TDbContext> _options;
    private TDbContext? _dbContext;

    public DatabaseContext(DbContextOptions<TDbContext> options)
    {
        _options = options;
    }

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class
    {
        EnsureDbContext();

        return _dbContext.Entry(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureDbContext();

        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public DbSet<TEntity> Set<TEntity>()
        where TEntity : class
    {
        EnsureDbContext();

        return _dbContext.Set<TEntity>();
    }

    [MemberNotNull(nameof(_dbContext))]
    private void EnsureDbContext()
    {
        if (_dbContext == null)
        {
            _dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), _options)
                ?? throw new InvalidOperationException($"Failed to create an instance of {typeof(TDbContext).Name}.");
        }
    }
}
