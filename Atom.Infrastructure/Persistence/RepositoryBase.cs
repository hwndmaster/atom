using System;
using System.Collections.Generic;
using System.Linq;
using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Persistence
{
    public interface IRepository<TEntity>
        where TEntity: EntityBase
    {
        void Delete(Guid entityId);
        void Overwrite(params TEntity[] entities);
        void Store(params TEntity[] entities);
    }

    public abstract class RepositoryBase<TEntity> : IRepository<TEntity>, IEntityQueryService<TEntity>
        where TEntity: EntityBase
    {
        protected readonly IEventBus _eventBus;
        protected readonly ILogger _logger;
        protected readonly IJsonPersister _persister;

        private List<TEntity> _entities;

        private readonly string FILENAME = @$".\Data\{typeof(TEntity).Name}.json";

        protected RepositoryBase(IEventBus eventBus, IJsonPersister persister, ILogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            _persister = persister;
        }

        private void EnsureInitialization()
        {
            if (_entities != null)
            {
                return;
            }

            _entities = _persister.LoadCollection<TEntity>(FILENAME).ToList();
            FillupRelations();
        }

        public IEnumerable<TEntity> GetAll()
        {
            EnsureInitialization();

            return _entities;
        }

        public TEntity FindById(Guid entityId)
        {
            EnsureInitialization();

            return _entities.FirstOrDefault(x => x.Id == entityId);
        }

        public void Delete(Guid entityId)
        {
            EnsureInitialization();

            DeleteInternal(entityId, FILENAME);
        }

        private void DeleteInternal(Guid entityId, string fileName)
        {
            var entity = _entities.FirstOrDefault(x => x.Id == entityId);
            if (entity == null)
            {
                _logger.LogWarning($"Cannot find entity '{entityId}' to delete");
                return;
            }

            _entities.Remove(entity);

            _persister.Store(fileName, _entities);

            _eventBus.Publish(new EntitiesDeletedEvent(typeof(TEntity), new [] { entityId }));
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
            var addedEntities = new List<EntityBase>();
            var updatedEntities = new List<EntityBase>();

            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }

                FillupRelations(entity);

                var index = _entities.FindIndex(x => x.Id == entity.Id);
                if (index == -1)
                {
                    addedEntities.Add(entity);
                    _entities.Add(entity);
                    _logger.LogTrace($"New entity '{entity}' with id '{entity.Id}' added");
                }
                else
                {
                    updatedEntities.Add(entity);
                    _entities[index] = entity;
                }
            }

            Guid[] deletedEntities = null;
            if (overwrite)
            {
                var allEntitiesId = entities.Select(x => x.Id).ToHashSet();
                var removedEntities = _entities.Where(x => !allEntitiesId.Contains(x.Id)).ToList();
                _entities.RemoveAll(x => !allEntitiesId.Contains(x.Id));
                deletedEntities = removedEntities.Select(x => x.Id).ToArray();
            }

            _persister.Store(FILENAME, _entities);

            if (addedEntities.Any())
                _eventBus.Publish(new EntitiesAddedEvent(addedEntities));
            if (updatedEntities.Any())
                _eventBus.Publish(new EntitiesUpdatedEvent(updatedEntities));
            if (deletedEntities?.Any() == true)
                _eventBus.Publish(new EntitiesDeletedEvent(typeof(TEntity), deletedEntities));

            _logger.LogInformation($"Entities of type {typeof(TEntity).Name} updated.");
        }

        protected void FillupRelations()
        {
            foreach (var entity in _entities)
            {
                FillupRelations(entity);
            }
        }

        protected virtual void FillupRelations(TEntity entity)
        {
        }
    }
}
