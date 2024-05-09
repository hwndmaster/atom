using Genius.Atom.Infrastructure.Entities;
using Genius.Atom.Infrastructure.TestingUtil;

namespace Genius.Atom.Data.Tests.Persistence;

public sealed class EntityQueryServiceProxyTests
{
    private readonly Fixture _fixture = InfrastructureTestHelper.CreateFixture();

    [Fact]
    public async Task CreateForType_ProducesCorrectQueryServiceInstance()
    {
        // Arrange
        var serviceProvider = new TestServiceProvider();
        var queryService = new SampleEntityQueryService();
        serviceProvider.RegisterInstance<IQueryService<SampleEntity>>(queryService);
        var type = typeof(SampleEntity);
        foreach (var entity in _fixture.CreateMany<SampleEntity>())
            queryService.Add(entity);

        // Act
        var sut = EntityQueryServiceProxy.CreateForType(type, serviceProvider);
        var actualGetAll = await sut.GetAllAsync();
        var actualFindById = await sut.FindByIdAsync(queryService.Entities.Last().Key);

        // Verify
        Assert.Equal(queryService.Entities.Values.Select(x => x), actualGetAll);
        Assert.Equal(queryService.Entities.Values.Last(), actualFindById);
    }

    private class SampleEntity : EntityBase
    {
    }

    private class SampleEntityQueryService : IQueryService<SampleEntity>
    {
        public readonly Dictionary<Guid, SampleEntity> Entities = new();

        internal void Add(SampleEntity entity) => Entities.Add(entity.Id, entity);

        public Task<SampleEntity?> FindByIdAsync(Guid entityId)
            => Task.FromResult((SampleEntity?)Entities[entityId]);

        public Task<IEnumerable<SampleEntity>> GetAllAsync()
            => Task.FromResult(Entities.Values.Select(x => x));
    }
}
