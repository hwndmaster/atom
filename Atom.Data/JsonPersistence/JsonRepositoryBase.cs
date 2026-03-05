using System.Reactive.Subjects;
using Genius.Atom.Data.IdHandlers;
using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.JsonPersistence;

public interface IJsonRepository<in TKey, in TReference, in TEntity>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TEntity : EntityBase<TKey, TReference>
{
    Task DeleteAsync(TReference entityId);
    Task OverwriteAsync(params TEntity[] entities);
    Task StoreAsync(params TEntity[] entities);
}

public abstract class JsonRepositoryBase<TKey, TReference, TEntity>
    : IJsonRepository<TKey, TReference, TEntity>, IDisposable
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TEntity: EntityBase<TKey, TReference>
{
    private readonly ReaderWriterLockSlim _initializationLocker = new();
    private readonly IEventBus _eventBus;
    private readonly Subject<IReadOnlyList<TEntity>> _loaded = new();
    private readonly IJsonPersister _persister;
    private readonly IIdHandler<TKey> _idHandler;
    protected readonly ILogger Logger;

    private List<TEntity>? _entities;
    private readonly string FILENAME = @$".\Data\{typeof(TEntity).Name}.json";

    protected JsonRepositoryBase(IEventBus eventBus, IJsonPersister persister, IIdHandler<TKey> idHandler, ILogger logger)
    {
        _eventBus = eventBus;
        _persister = persister;
        _idHandler = idHandler;
        Logger = logger;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        await EnsureInitializationAsync();

        return _entities.NotNull();
    }

    public virtual async Task<TEntity?> FindByIdAsync(TReference entityId)
    {
        await EnsureInitializationAsync();

        return _entities.NotNull().Find(x => x.Id.Equals(entityId));
    }

    public virtual async Task DeleteAsync(TReference entityId)
    {
        await EnsureInitializationAsync();

        DeleteInternal(entityId, FILENAME);
    }

    public virtual Task OverwriteAsync(params TEntity[] entities)
    {
        return StoreInternalAsync(true, entities);
    }

    public virtual Task StoreAsync(params TEntity[] entities)
    {
        return StoreInternalAsync(false, entities);
    }

    protected virtual void Dispose(bool disposing)
    {
        _loaded.Dispose();
        _initializationLocker.Dispose();
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

    private async Task EnsureInitializationAsync()
    {
        _initializationLocker.EnterWriteLock();

        if (_entities is not null)
        {
            _initializationLocker.ExitWriteLock();
            return;
        }

        _entities = _persister.LoadCollection<TEntity>(FILENAME).NotNull().ToList();
        await FillUpRelationsAsync();
        _initializationLocker.ExitWriteLock();
        _loaded.OnNext(_entities.AsReadOnly());
    }

    private void DeleteInternal(TReference entityId, string fileName)
    {
        Guard.NotNull(_entities);

        var entity = _entities.Find(x => x.Id.Equals(entityId));
        if (entity is null)
        {
            Logger.LogWarning("Cannot find entity '{EntityId}' to delete", entityId);
            return;
        }

        _entities.Remove(entity);

        _persister.Store(fileName, _entities);

        _eventBus.Publish(new EntitiesAffectedEvent<TReference>(typeof(TEntity), EntityAffectedEventType.Deleted, entityId));
    }

    private async Task StoreInternalAsync(bool overwrite, params TEntity[] entities)
    {
        if (entities is null)
        {
            return;
        }

        await EnsureInitializationAsync();

        var addedEntities = new List<TReference>();
        var updatedEntities = new List<TReference>();

        foreach (var entity in entities)
        {
            if (entity.Id.IsDefault())
            {
                entity.SetId(TReference.Create(_idHandler.GenerateId()));
            }

            await FillUpRelationsAsync(entity);

            var index = _entities!.FindIndex(x => x.Id.Equals(entity.Id));
            if (index == -1)
            {
                addedEntities.Add(entity.Id);
                _entities.Add(entity);
                Logger.LogTrace("New entity '{Entity}' with id '{EntityId}' added", entity, entity.Id);
            }
            else
            {
                updatedEntities.Add(entity.Id);
                _entities[index] = entity;
            }
        }

        TReference[]? deletedEntities = Array.Empty<TReference>();
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
            _eventBus.Publish(new EntitiesAffectedEvent<TReference>(typeof(TEntity), addedEntities,
                updatedEntities, deletedEntities));
        }

        Logger.LogInformation("Entities of type {TypeName} updated.", typeof(TEntity).Name);
    }

    protected IObservable<IReadOnlyList<TEntity>> Loaded => _loaded;
}
