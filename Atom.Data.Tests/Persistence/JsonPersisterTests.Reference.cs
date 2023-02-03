using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.TestingUtil;

namespace Genius.Atom.Data.Tests.Persistence;

public sealed partial class JsonPersisterTests
{
    [Fact]
    public void StoreAndLoad_WithReference()
    {
        // Arrange
        var file = _fixture.Create<string>();
        var entity = _fixture.Create<SampleClassWithEntity>();
        var referencedEntity = _fixture.Create<ReferencedEntity>();
        var serviceProvider = new TestServiceProvider();
        serviceProvider.RegisterSingleton<IQueryService<ReferencedEntity>, ReferencedEntityQueryService>();
        Module.Initialize(serviceProvider);
        var sut = CreateSystemUnderTest();

        // Act
        // Expected JSON structure:
        // {
        //   "GrandEntity": {
        //     "ReferencedEntity": "{ID}",
        //     "NonReferencedEntity": {
        //       ...
        //     }
        //     "ReferencedEntities": [
        //       "{ID}",
        //       "{ID}",
        //       "{ID}"
        //     ]
        //     "NonReferencedEntities": [
        //       { ... },
        //       { ... },
        //       { ... }
        //     ]
        //   }
        // }
        sut.Store(file, entity);
        var result = sut.Load<SampleClassWithEntity>(file);

        // Verify
        Assert.NotNull(result);
        Assert.Equal(entity.GrandEntity.Id, result.GrandEntity.Id);
        Assert.Equal(entity.GrandEntity.ReferencedEntity, referencedEntity);

        // TODO: add asserts for other props
        Assert.Equal(entity.GrandEntity.ReferencedEntity, referencedEntity);
    }

    private sealed class EntityGrand : EntityBase
    {
        [Reference]
        public required ReferencedEntity ReferencedEntity { get; set; }

        public required ReferencedEntity NonReferencedEntity { get; set; }

        [Reference]
        public List<ReferencedEntity> ReferencedEntities { get; set; } = new List<ReferencedEntity>();

        public List<ReferencedEntity> NonReferencedEntities { get; set; } = new List<ReferencedEntity>();
    }

    private sealed class ReferencedEntity : EntityBase
    {
        public int IntValue { get; set; }
    }

    private sealed class SampleClassWithEntity
    {
        public required EntityGrand GrandEntity { get; set; }
    }

    private sealed class ReferencedEntityQueryService : IQueryService<ReferencedEntity>
    {
        private readonly Dictionary<Guid, ReferencedEntity> _entities = new();

        internal void Add(ReferencedEntity entity) => _entities.Add(entity.Id, entity);

        public Task<ReferencedEntity?> FindByIdAsync(Guid entityId)
            => Task.FromResult((ReferencedEntity?)_entities[entityId]);

        public Task<IEnumerable<ReferencedEntity>> GetAllAsync()
            => Task.FromResult(_entities.Values.Select(x => x));
    }
}
