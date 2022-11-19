using Genius.Atom.Data.Persistence;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.Infrastructure.TestingUtil.Io;

namespace Genius.Atom.Infrastructure.Tests.Persistence;

public sealed class JsonPersisterTests
{
    private readonly Fixture _fixture = new();
    private readonly TestServiceProvider _serviceProvider = new();
    private readonly TypeDiscriminators _typeDiscriminators;

    public JsonPersisterTests()
    {
        _typeDiscriminators = new(_serviceProvider);
    }

    [Fact]
    public void StoreAndLoadCollection_SimpleScenario()
    {
        // Arrange
        var file = _fixture.Create<string>();
        var entities = _fixture.CreateMany<SimpleClass>().ToList();
        var sut = CreateSystemUnderTest();

        // Act
        sut.Store(file, entities);
        var result = sut.LoadCollection<SimpleClass>(file);

        // Verify
        Assert.NotNull(result);
        Assert.Equal(entities.Count, result.Length);
        for (var i = 0; i < result.Length; i++)
        {
            Assert.Equal(entities[i].IntValue, result[i].IntValue);
        }
    }

    [Fact]
    public void StoreAndLoadCollection_DiscriminatedClassesScenario()
    {
        // Arrange
        _typeDiscriminators.AddMapping<DerivedClassA>("derived-1");
        _typeDiscriminators.AddMapping<DerivedClassB>("derived-2");

        var file = _fixture.Create<string>();
        var entities = new AbstractClass[]
        {
            _fixture.Create<DerivedClassA>(),
            _fixture.Create<DerivedClassB>()
        };
        var sut = CreateSystemUnderTest();

        // Act
        sut.Store(file, entities);
        var result = sut.LoadCollection<AbstractClass>(file);

        // Verify
        Assert.NotNull(result);
        Assert.Equal(entities.Length, result.Length);
        for (var i = 0; i < result.Length; i++)
        {
            Assert.Equal(entities[i].IntValue, result[i].IntValue);
            if (result[i] is DerivedClassA derA && entities[i] is DerivedClassA derB)
            {
                Assert.Equal(derB.AnotherValue1, derA.AnotherValue1);
            }
            else if (result[i] is DerivedClassB derC && entities[i] is DerivedClassB derD)
            {
                Assert.Equal(derC.AnotherValue2, derD.AnotherValue2);
            }
            else
            {
                Assert.Fail("Can't match the result class type.");
            }
        }
    }

    [Fact]
    public void StoreAndLoad_UpgradeVersionScenario()
    {
        // Arrange
        _serviceProvider.RegisterSingleton<DerivedClassAVersion1To2Upgrader>();
        _typeDiscriminators.AddMapping<DerivedClassA>("derived-1", 1);
        _typeDiscriminators.AddMapping<DerivedClassAVersion2, DerivedClassA, DerivedClassAVersion1To2Upgrader>("derived-1", 2);

        var file = _fixture.Create<string>();
        var entity = _fixture.Create<DerivedClassA>();
        var sut = CreateSystemUnderTest();

        // Act
        sut.Store(file, entity);
        var result = sut.Load<DerivedClassAVersion2>(file);

        // Verify
        Assert.NotNull(result);
        Assert.Equal(entity.IntValue, result.IntValue);
        Assert.Equal(entity.AnotherValue1, result.AnotherValue3);
    }

    [Fact]
    public void Store_WhenNoTypeDiscriminatorAvailable_ThrowsException()
    {
        // Arrange
        // We don't add mapping for `DerivedClass1`
        _typeDiscriminators.AddMapping<DerivedClassB>("derived-2");

        var file = _fixture.Create<string>();
        var entities = new AbstractClass[]
        {
            _fixture.Create<DerivedClassA>(),
            _fixture.Create<DerivedClassB>()
        };
        var sut = CreateSystemUnderTest();

        // Act & Verify
        Assert.Throws<InvalidOperationException>(() => sut.Store(file, entities));
    }

    private JsonPersister CreateSystemUnderTest(params IJsonConverter[] converters)
    {
        return new(new TestFileService(), _typeDiscriminators, converters);
    }

    private sealed class SimpleClass
    {
        public int IntValue { get; set; }
    }

    private abstract class AbstractClass
    {
        public required int IntValue { get; init; }
    }

    private sealed class DerivedClassA : AbstractClass
    {
        public required int AnotherValue1 { get; init; }
    }

    private sealed class DerivedClassAVersion2 : AbstractClass
    {
        public required int AnotherValue3 { get; init; }
    }

    private sealed class DerivedClassB : AbstractClass
    {
        public required int AnotherValue2 { get; init; }
    }

    private sealed class DerivedClassAVersion1To2Upgrader : IDataVersionUpgrader<DerivedClassA, DerivedClassAVersion2>
    {
        public DerivedClassAVersion2 Upgrade(DerivedClassA value)
        {
            return new DerivedClassAVersion2
            {
                IntValue = value.IntValue,
                AnotherValue3 = value.AnotherValue1
            };
        }
    }
}
