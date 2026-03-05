using System.Linq.Expressions;
using Genius.Atom.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef;

internal abstract class BaseRepository<TEntity, TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    : IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TKey : notnull
    where TEntity : EntityBase<TKey, TReference>
    where TReference : IReference<TKey, TReference>
    where TUpdateDto: IPrimaryId<TKey, TReference>, ITimeStamped
{
    private readonly IDateTime _dateTime;
    private readonly IDbContextProvider _dbContextProvider;

    protected BaseRepository(IDateTime dateTime, IDbContextProvider dbContextProvider)
    {
        _dateTime = dateTime.NotNull();
        _dbContextProvider = dbContextProvider.NotNull();
    }

    public async Task<TGetDto> GetByIdAsync(TReference id, DbContext? context = null, CancellationToken cancellationToken = default)
    {
        bool hasContext = context is not null;
        context ??= _dbContextProvider.GetDbContext();
        await using var dbContext = context;

        try
        {
            return await dbContext.Set<TEntity>()
                .Where(IdEquals(id))
                .Select(ProjectToGetDto())
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Entity with ID '{id}' not found.");
        }
        finally
        {
            if (!hasContext)
            {
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    public async Task<IEnumerable<TGetDto>> GetAllAsync(DbContext? context = null, CancellationToken cancellationToken = default)
    {
        bool hasContext = context is not null;
        context ??= _dbContextProvider.GetDbContext();
        await using var dbContext = context;

        try
        {
            return await dbContext.Set<TEntity>()
            .Select(ProjectToGetDto())
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (!hasContext)
            {
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    public async Task<CreatedEntityDto<TKey, TReference>> CreateAsync(TCreateDto createDto, DbContext? context = null, CancellationToken cancellationToken = default)
    {
        bool hasContext = context is not null;
        context ??= _dbContextProvider.GetDbContext();
        await using var dbContext = context;

        try
        {
            var entity = MapCreateDto(createDto, dbContext);
            var date = TruncateToSeconds(_dateTime.NowUtc);
            entity = entity with {
                DateCreated = entity.DateCreated == DateTimeOffset.MinValue ? date : entity.DateCreated,
                LastModified = entity.LastModified == DateTimeOffset.MinValue ? date : entity.LastModified
            };
            dbContext.Set<TEntity>().Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await AfterCreateAsync(createDto, entity, dbContext, cancellationToken);

            return new CreatedEntityDto<TKey, TReference>(entity.Id, entity.LastModified);
        }
        finally
        {
            if (!hasContext)
            {
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    public async Task<UpdatedEntityDto<TKey, TReference>> UpdateAsync(TUpdateDto updateDto, DbContext? context = null, CancellationToken cancellationToken = default)
    {
        updateDto.NotNull();

        bool hasContext = context is not null;
        context ??= _dbContextProvider.GetDbContext();
        await using var dbContext = context;

        try
        {
            var existingEntity = await dbContext.Set<TEntity>()
                .FirstOrDefaultAsync(IdEquals(updateDto.Id), cancellationToken).ConfigureAwait(false);

            if (existingEntity is null)
            {
                throw new InvalidOperationException($"Entity with ID {updateDto.Id} not found.");
            }

            if (existingEntity.LastModified != updateDto.LastModified)
            {
                throw new InvalidOperationException($"Entity with ID {updateDto.Id} has a version conflict.");
            }

            var updatedEntity = MapUpdateDto(updateDto, existingEntity, dbContext);

            // Update the time stamp which also represents the version
            updatedEntity = updatedEntity with { LastModified = TruncateToSeconds(_dateTime.NowUtc) };

            dbContext.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

            await AfterUpdateAsync(updateDto, updatedEntity, dbContext, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new UpdatedEntityDto<TKey, TReference>(updatedEntity.Id, updatedEntity.LastModified);
        }
        finally
        {
            if (!hasContext)
            {
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    public async Task DeleteAsync(TReference id, DbContext? context = null, CancellationToken cancellationToken = default)
    {
        bool hasContext = context is not null;
        context ??= _dbContextProvider.GetDbContext();
        await using var dbContext = context;
        try
        {
            var entity = await dbContext.Set<TEntity>()
                .FirstOrDefaultAsync(IdEquals(id), cancellationToken).ConfigureAwait(false);

            if (entity is null)
            {
                throw new InvalidOperationException($"Entity with ID '{id}' not found.");
            }

            dbContext.Set<TEntity>().Remove(entity);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (!hasContext)
            {
                await dbContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    protected DbContext WithDbContext() => _dbContextProvider.GetDbContext();

    protected abstract Expression<Func<TEntity, TGetDto>> ProjectToGetDto();
    protected abstract TEntity MapCreateDto(TCreateDto dto, DbContext dbContext);
    protected abstract TEntity MapUpdateDto(TUpdateDto dto, TEntity existingEntity, DbContext dbContext);
    protected virtual Task AfterCreateAsync(TCreateDto createRequest, TEntity createdEntity, DbContext dbContext, CancellationToken cancellationToken) => Task.CompletedTask;
    protected virtual Task AfterUpdateAsync(TUpdateDto updateRequest, TEntity updatedEntity, DbContext dbContext, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Builds an expression tree for <c>e => e.Id == id</c> without relying on the <c>==</c> operator
    /// being available on the generic <typeparamref name="TReference"/> at compile time.
    /// EF Core translates this to the appropriate SQL equality comparison using the value converter.
    /// </summary>
    private static Expression<Func<TEntity, bool>> IdEquals(TReference id)
    {
        var param = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(param, nameof(EntityBase<TKey, TReference>.Id));
        var constant = Expression.Constant(id, typeof(TReference));
        var equal = Expression.Equal(property, constant);
        return Expression.Lambda<Func<TEntity, bool>>(equal, param);
    }

    /// <summary>
    ///   Truncates a <see cref="DateTimeOffset"/> to whole seconds,
    ///   matching the precision of Unix timestamp storage in the database.
    /// </summary>
    private static DateTimeOffset TruncateToSeconds(DateTimeOffset value)
        => DateTimeOffset.FromUnixTimeSeconds(value.ToUnixTimeSeconds());
}
