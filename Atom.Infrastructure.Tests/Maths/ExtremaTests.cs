using Genius.Atom.Infrastructure.Maths;
using Genius.Atom.Infrastructure.TestingUtil;

namespace Genius.Atom.Infrastructure.Tests;

public sealed class ExtremaTests
{
    private readonly IFixture _fixture = InfrastructureTestHelper.CreateFixture();

    [Fact]
    public void Extrema_HappyFlowScenario()
    {
        // Arrange
        var sample = _fixture.CreateMany<int>(10).ToList();

        // Act
        var actual = sample.Extrema();

        // Verify
        var expectedMin = sample.Min();
        var expectedMax = sample.Max();
        Assert.Equal(expectedMin, actual.Minimum);
        Assert.Equal(expectedMax, actual.Maximum);
    }

    [Fact]
    public void ExtremaAndCount_HappyFlowScenario()
    {
        // Arrange
        const int count = 10;
        var sample = _fixture.CreateMany<int>(count).ToList();

        // Act
        var actual = sample.ExtremaAndCount();

        // Verify
        var expectedMin = sample.Min();
        var expectedMax = sample.Max();
        Assert.Equal(expectedMin, actual.Minimum);
        Assert.Equal(expectedMax, actual.Maximum);
        Assert.Equal(count, actual.Count);
    }
}
