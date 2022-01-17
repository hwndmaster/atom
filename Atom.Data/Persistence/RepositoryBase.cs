using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Persistence;

public interface IRepository<TEntity>
    where TEntity: EntityBase
{
    Task DeleteAsync(Guid entityId);
    Task OverwriteAsync(params TEntity[] entities);
    Task StoreAsync(params TEntity[] entities);
}

public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    where TEntity: EntityBase
{
    protected readonly IEventBus _eventBus;
    protected readonly ILogger _logger;
    protected readonly IJsonPersister _persister;

    private List<TEntity>? _entities;

    private readonly string FILENAME = @$".\Data\{typeof(TEntity).Name}.json";

    protected RepositoryBase(IEventBus eventBus, IJsonPersister persister, ILogger logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _persister = persister;
    }

    private async Task EnsureInitializationAsync()
    {
        if (_entities is not null)
        {
            return;
        }

        _entities = _persister.LoadCollection<TEntity>(FILENAME).NotNull().ToList();
        await FillUpRelationsAsync();
    }

    protected async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        await EnsureInitializationAsync();

        return _entities.NotNull();
    }

    protected async Task<TEntity?> FindByIdAsync(Guid entityId)
    {
        await EnsureInitializationAsync();

        return _entities.NotNull().Find(x => x.Id == entityId);
    }

    public async Task DeleteAsync(Guid entityId)
    {
        await EnsureInitializationAsync();

        DeleteInternal(entityId, FILENAME);
    }

    private void DeleteInternal(Guid entityId, string fileName)
    {
        Guard.NotNull(_entities);

        var entity = _entities.Find(x => x.Id == entityId);
        if (entity is null)
        {
            _logger.LogWarning("Cannot find entity '{entityId}' to delete", entityId);
            return;
        }

        _entities.Remove(entity);

        _persister.Store(fileName, _entities);

        _eventBus.Publish(new EntitiesAffectedEvent(typeof(TEntity), EntityAffectedEventType.Deleted, entityId));
    }

    public Task OverwriteAsync(params TEntity[] entities)
    {
        return StoreInternalAsync(true, entities);
    }

    public Task StoreAsync(params TEntity[] entities)
    {
        return StoreInternalAsync(false, entities);
    }

    private async Task StoreInternalAsync(bool overwrite, params TEntity[] entities)
    {
        if (entities is null)
        {
            return;
        }

        await EnsureInitializationAsync();

        var addedEntities = new List<Guid>();
        var updatedEntities = new List<Guid>();

        foreach (var entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await FillUpRelationsAsync(entity);

            var index = _entities!.FindIndex(x => x.Id == entity.Id);
            if (index == -1)
            {
                addedEntities.Add(entity.Id);
                _entities.Add(entity);
                _logger.LogTrace("New entity '{entity}' with id '{entityId}' added", entity, entity.Id);
            }
            else
            {
                updatedEntities.Add(entity.Id);
                _entities[index] = entity;
            }
        }

        Guid[]? deletedEntities = Array.Empty<Guid>();
        if (overwrite)
        {
            var allEntitiesId = entities.Select(x => x.Id).ToHashSet();
            var removedEntities = _entities!.Where(x => !allEntitiesId.Contains(x.Id)).ToList();
            _entities!.RemoveAll(x => !allEntitiesId.Contains(x.Id));
            deletedEntities = removedEntities.Select(x => x.Id).ToArray();
        }

        _persister.Store(FILENAME, _entities!);

        if (addedEntities.Any() || updatedEntities.Any() || deletedEntities.Any())
        {
            _eventBus.Publish(new EntitiesAffectedEvent(typeof(TEntity), addedEntities,
                updatedEntities, deletedEntities));
        }

        _logger.LogInformation("Entities of type {typeName} updated.", typeof(TEntity).Name);
    }

    protected async Task FillUpRelationsAsync()
    {
        foreach (var entity in _entities!)
        {
            await FillUpRelationsAsync(entity);
        }
    }

    protected virtual Task FillUpRelationsAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }
}
