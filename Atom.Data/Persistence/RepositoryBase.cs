using Genius.Atom.Infrastructure;
using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Persistence;

public interface IRepository<TEntity>
    where TEntity: EntityBase
{
    void Delete(Guid entityId);
    void Overwrite(params TEntity[] entities);
    void Store(params TEntity[] entities);
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

    private void EnsureInitialization()
    {
        if (_entities is not null)
        {
            return;
        }

        _entities = _persister.LoadCollection<TEntity>(FILENAME).NotNull().ToList();
        FillUpRelations();
    }

    protected IEnumerable<TEntity> GetAll()
    {
        EnsureInitialization();

        return _entities!;
    }

    protected TEntity? FindById(Guid entityId)
    {
        EnsureInitialization();

        return _entities!.Find(x => x.Id == entityId);
    }

    public void Delete(Guid entityId)
    {
        EnsureInitialization();

        DeleteInternal(entityId, FILENAME);
    }

    private void DeleteInternal(Guid entityId, string fileName)
    {
        var entity = _entities!.Find(x => x.Id == entityId);
        if (entity is null)
        {
            _logger.LogWarning("Cannot find entity '{entityId}' to delete", entityId);
            return;
        }

        _entities.Remove(entity);

        _persister.Store(fileName, _entities);

        _eventBus.Publish(new EntitiesDeletedEvent(typeof(TEntity).Name, new [] { entityId }));
    }

    public void Overwrite(params TEntity[] entities)
    {
        StoreInternal(true, entities);
    }

    public void Store(params TEntity[] entities)
    {
        StoreInternal(false, entities);
    }

    private void StoreInternal(bool overwrite, params TEntity[] entities)
    {
        EnsureInitialization();

        var addedEntities = new List<Guid>();
        var updatedEntities = new List<Guid>();

        foreach (var entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            FillUpRelations(entity);

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

        Guid[]? deletedEntities = null;
        if (overwrite)
        {
            var allEntitiesId = entities.Select(x => x.Id).ToHashSet();
            var removedEntities = _entities!.Where(x => !allEntitiesId.Contains(x.Id)).ToList();
            _entities!.RemoveAll(x => !allEntitiesId.Contains(x.Id));
            deletedEntities = removedEntities.Select(x => x.Id).ToArray();
        }

        _persister.Store(FILENAME, _entities!);

        if (addedEntities.Any())
            _eventBus.Publish(new EntitiesAddedEvent(addedEntities));
        if (updatedEntities.Any())
            _eventBus.Publish(new EntitiesUpdatedEvent(updatedEntities));
        if (deletedEntities?.Any() == true)
            _eventBus.Publish(new EntitiesDeletedEvent(typeof(TEntity).Name, deletedEntities));

        _logger.LogInformation("Entities of type {typeName} updated.", typeof(TEntity).Name);
    }

    protected void FillUpRelations()
    {
        foreach (var entity in _entities!)
        {
            FillUpRelations(entity);
        }
    }

    protected virtual void FillUpRelations(TEntity entity)
    {
    }
}
