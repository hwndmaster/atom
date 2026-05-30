using System.Linq.Expressions;
using Genius.Atom.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef;

/// <summary>
/// Provides a base implementation of the <see cref="IRepository{TKey, TReference, TGetDto, TCreateDto, TUpdateDto}"/> interface.
/// It is highly recommended to register derived repository classes as transient or scoped
/// services in the dependency injection container to ensure proper lifetime management
/// of the database context.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TReference">The type of the entity's reference.</typeparam>
/// <typeparam name="TGetDto">The type of the DTO used for retrieving entities.</typeparam>
/// <typeparam name="TCreateDto">The type of the DTO used for creating entities.</typeparam>
/// <typeparam name="TUpdateDto">The type of the DTO used for updating entities.</typeparam>
public abstract class BaseRepository<TEntity, TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    : IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TKey : notnull
    where TEntity : EntityBase<TKey, TReference>
    where TReference : IReference<TKey, TReference>
    where TUpdateDto: IPrimaryId<TKey, TReference>, ITimeStamped
{
    private readonly IDateTime _dateTime;
    private readonly IDatabaseContext _databaseContext;
    private Expression<Func<TEntity, TGetDto>>? _projectToGetDto;

    protected BaseRepository(IDateTime dateTime, IDatabaseContext databaseContext)
    {
        _dateTime = dateTime.NotNull();
        _databaseContext = databaseContext.NotNull();
    }

    public async Task<TGetDto?> GetByIdAsync(TReference id, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Set<TEntity>()
            .AsNoTracking()
            .Where(IdEquals(id))
            .Select(GetProjectionToGetDto())
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TGetDto> GetByIdOrThrowAsync(TReference id, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Set<TEntity>()
            .AsNoTracking()
            .Where(IdEquals(id))
            .Select(GetProjectionToGetDto())
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Entity with ID '{id}' not found.");
    }

    public async Task<IEnumerable<TGetDto>> GetByIdsAsync(IEnumerable<TReference> ids, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Set<TEntity>()
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(GetProjectionToGetDto())
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Set<TEntity>()
            .AsNoTracking()
            .Select(GetProjectionToGetDto())
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreatedEntityDto<TKey, TReference>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = MapCreateDto(createDto);
        var date = TruncateToSeconds(_dateTime.NowUtc);
        entity = entity with {
            DateCreated = entity.DateCreated == DateTimeOffset.MinValue ? date : entity.DateCreated,
            LastModified = entity.LastModified == DateTimeOffset.MinValue ? date : entity.LastModified
        };
        _databaseContext.Set<TEntity>().Add(entity);
        await _databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await AfterCreateAsync(createDto, entity, cancellationToken).ConfigureAwait(false);

        return new CreatedEntityDto<TKey, TReference>(entity.Id, entity.LastModified);
    }

    public async Task<UpdatedEntityDto<TKey, TReference>> UpdateAsync(TUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        updateDto.NotNull();

        var existingEntity = await _databaseContext.Set<TEntity>()
            .FirstOrDefaultAsync(IdEquals(updateDto.Id), cancellationToken).ConfigureAwait(false);

        if (existingEntity is null)
        {
            throw new InvalidOperationException($"Entity with ID {updateDto.Id} not found.");
        }

        if (existingEntity.LastModified != updateDto.LastModified)
        {
            throw new InvalidOperationException($"Entity with ID {updateDto.Id} has a version conflict.");
        }

        var updatedEntity = MapUpdateDto(updateDto, existingEntity);

        // Preserve the primary key from the tracked entity (PK must not change),
        // and update the time stamp which also represents the version.
        updatedEntity = updatedEntity with { Id = existingEntity.Id, LastModified = TruncateToSeconds(_dateTime.NowUtc) };

        _databaseContext.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);

        await AfterUpdateAsync(updateDto, updatedEntity, cancellationToken);

        await _databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new UpdatedEntityDto<TKey, TReference>(updatedEntity.Id, updatedEntity.LastModified);
    }

    public async Task DeleteAsync(TReference id, CancellationToken cancellationToken = default)
    {
        var entity = await _databaseContext.Set<TEntity>()
            .FirstOrDefaultAsync(IdEquals(id), cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            throw new InvalidOperationException($"Entity with ID '{id}' not found.");
        }

        _databaseContext.Set<TEntity>().Remove(entity);
        await _databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the database context.
    /// </summary>
    protected IDatabaseContext GetContext() => _databaseContext;

    /// <summary>
    /// Returns a cached expression tree for projecting <typeparamref name="TEntity"/> to <typeparamref name="TGetDto"/>.
    /// </summary>
    protected Expression<Func<TEntity, TGetDto>> GetProjectionToGetDto()
        => _projectToGetDto ??= ProjectToGetDto;

    protected abstract TEntity MapCreateDto(TCreateDto dto);
    protected abstract TEntity MapUpdateDto(TUpdateDto dto, TEntity existingEntity);
    protected virtual Task AfterCreateAsync(TCreateDto createRequest, TEntity createdEntity, CancellationToken cancellationToken) => Task.CompletedTask;
    protected virtual Task AfterUpdateAsync(TUpdateDto updateRequest, TEntity updatedEntity, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Defines an expression tree for projecting <typeparamref name="TEntity"/> to <typeparamref name="TGetDto"/>.
    /// </summary>
    protected abstract Expression<Func<TEntity, TGetDto>> ProjectToGetDto { get; }

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
