using System.Text.Json;
using Genius.Atom.Data.JsonConverters;

namespace Genius.Atom.Data.Tests.JsonConverters;

public sealed class ReferenceConverterTests
{
    private static readonly Guid SampleGuid = Guid.Parse("8f5c1a1e-3d2b-4c9f-9a7e-6b0d4f2c1a55");

    [Fact]
    public void Int32Reference_Write_ProducesANumber()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var json = JsonSerializer.Serialize(new IntHolder(new IntItemRef(42)), options);

        // Assert
        Assert.Equal("""{"Id":42}""", json);
    }

    [Fact]
    public void Int32Reference_Read_ParsesANumber()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var holder = JsonSerializer.Deserialize<IntHolder>("""{"Id":42}""", options);

        // Assert
        Assert.Equal(new IntItemRef(42), holder!.Id);
    }

    [Fact]
    public void Int32Reference_Read_GivenANonNumericToken_Throws()
    {
        // Arrange
        var options = CreateOptions();

        // Act & Assert
        var ex = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<IntHolder>("""{"Id":"42"}""", options));
        Assert.Equal("Expected a numeric ID for reference.", ex.Message);
    }

    [Fact]
    public void GuidReference_Write_ProducesAString()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var json = JsonSerializer.Serialize(new GuidHolder(new GuidItemRef(SampleGuid)), options);

        // Assert
        Assert.Equal($"{{\"Id\":\"{SampleGuid}\"}}", json);
    }

    [Fact]
    public void GuidReference_Read_ParsesAString()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var holder = JsonSerializer.Deserialize<GuidHolder>($"{{\"Id\":\"{SampleGuid}\"}}", options);

        // Assert
        Assert.Equal(new GuidItemRef(SampleGuid), holder!.Id);
    }

    [Fact]
    public void GuidReference_Read_GivenANonStringToken_Throws()
    {
        // Arrange
        var options = CreateOptions();

        // Act & Assert
        var ex = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<GuidHolder>("""{"Id":42}""", options));
        Assert.Equal("Expected a GUID string for reference.", ex.Message);
    }

    [Fact]
    public void BothConverters_RoundTripTheirReferences()
    {
        // Arrange
        var options = CreateOptions();
        var intHolder = new IntHolder(new IntItemRef(7));
        var guidHolder = new GuidHolder(new GuidItemRef(SampleGuid));

        // Act
        var reloadedInt = JsonSerializer.Deserialize<IntHolder>(JsonSerializer.Serialize(intHolder, options), options);
        var reloadedGuid = JsonSerializer.Deserialize<GuidHolder>(JsonSerializer.Serialize(guidHolder, options), options);

        // Assert
        Assert.Equal(intHolder, reloadedInt);
        Assert.Equal(guidHolder, reloadedGuid);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ReferenceConverter<IntItemRef>());
        options.Converters.Add(new GuidReferenceConverter<GuidItemRef>());
        return options;
    }

    private sealed record IntItemRef(int Id) : IReference<int, IntItemRef>
    {
        public static IntItemRef Create(int id) => new(id);
    }

    private sealed record GuidItemRef(Guid Id) : IReference<Guid, GuidItemRef>
    {
        public static GuidItemRef Create(Guid id) => new(id);
    }

    private sealed record IntHolder(IntItemRef Id);

    private sealed record GuidHolder(GuidItemRef Id);
}
