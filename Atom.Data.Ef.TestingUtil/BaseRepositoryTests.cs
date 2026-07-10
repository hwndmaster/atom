using Genius.Atom.Infrastructure.TestingUtil;
using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef.TestingUtil;

public abstract class BaseRepositoryTests<TKey, TReference, TGetDto, TCreateDto, TUpdateDto, TRepository, TDbContext>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TGetDto : IPrimaryId<TKey, TReference>, ITimeStamped
    where TUpdateDto : IPrimaryId<TKey, TReference>, ITimeStamped
    where TRepository : IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TDbContext : DbContext
{
    private readonly Lazy<TRepository> _repository;
    private readonly DatabaseContext<TDbContext> _databaseContext;
    private readonly DbContextOptions<TDbContext> _dbOptions;
    private int _index;

    protected FakeDateTime FakeDateTime = new();
    protected TRepository Repository => _repository.Value;

    protected BaseRepositoryTests()
    {
        _dbOptions = new DbContextOptionsBuilder<TDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContext = new DatabaseContext<TDbContext>(_dbOptions);
        _repository = new Lazy<TRepository>(() => CreateRepository(_databaseContext));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsResult()
    {
        // Arrange
        await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        var created = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = await Repository.GetByIdAsync(created.EntityId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.EntityId, result.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsResults()
    {
        // Arrange
        var created1 = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        var created2 = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        var created3 = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var result = (await Repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken)).ToArray();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Contains(result, r => r.Id.Equals(created1.EntityId));
        Assert.Contains(result, r => r.Id.Equals(created2.EntityId));
        Assert.Contains(result, r => r.Id.Equals(created3.EntityId));
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedEntity()
    {
        // Arrange
        var createDto = CreateSampleCreateDto(_index);

        // Act
        var result = await Repository.CreateAsync(createDto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(default(TKey), result.EntityId.Id);
        Assert.NotEqual(default, result.LastModified);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedEntity()
    {
        // Arrange
        var created = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        var updateDto = CreateSampleUpdateDto(created.EntityId.Id, created.LastModified, ++_index);
        FakeDateTime.Advance(TimeSpan.FromMinutes(5)); // Ensure LastModified will be different after update

        // Act
        var result = await Repository.UpdateAsync(updateDto, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.EntityId.Id, result.EntityId.Id);
        Assert.NotEqual(created.LastModified, result.LastModified);
    }

    [Fact]
    public async Task UpdateAsync_WhenVersionConflicts_ThrowsEntityVersionConflictException()
    {
        // Arrange
        var created = await Repository.CreateAsync(CreateSampleCreateDto(++_index), cancellationToken: TestContext.Current.CancellationToken);
        var staleLastModified = created.LastModified.AddSeconds(-1);
        var updateDto = CreateSampleUpdateDto(created.EntityId.Id, staleLastModified, ++_index);

        // Act
        var exception = await Record.ExceptionAsync(
            () => Repository.UpdateAsync(updateDto, cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        var conflict = Assert.IsType<EntityVersionConflictException>(exception);
        Assert.Equal((object)created.EntityId.Id, conflict.Id);
        Assert.Equal(created.LastModified, conflict.StoredLastModified);
        Assert.Equal(staleLastModified, conflict.AttemptedLastModified);
    }

    [Fact]
    public async Task DeleteAsync_CompletesSuccessfully()
    {
        // Arrange
        var created = await Repository.CreateAsync(CreateSampleCreateDto(_index), cancellationToken: TestContext.Current.CancellationToken);

        // Act
        var exception = await Record.ExceptionAsync(
            () => Repository.DeleteAsync(TReference.Create(created.EntityId.Id), cancellationToken: TestContext.Current.CancellationToken));

        // Assert
        Assert.Null(exception);
    }

    protected abstract TRepository CreateRepository(IDatabaseContext databaseContext);
    protected abstract TCreateDto CreateSampleCreateDto(int index = 0);
    protected abstract TUpdateDto CreateSampleUpdateDto(TKey id, DateTimeOffset lastModified, int index = 0);
}
