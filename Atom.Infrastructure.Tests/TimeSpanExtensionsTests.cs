namespace Genius.Atom.Infrastructure.Tests;

public sealed class TimeSpanExtensionsTests
{
    [Theory]
    [InlineData(999, 0, 0, "999 min")]
    [InlineData(999, 59, 999, "999 min 59 sec 999 ms")]
    [InlineData(5, 0, 0, "5 min")]
    [InlineData(5, 40, 0, "5 min 40 sec")]
    [InlineData(1, 2, 3, "1 min 2 sec 3 ms")]
    [InlineData(0, 2, 3, "2 sec 3 ms")]
    [InlineData(0, 0, 999, "0 sec 999 ms")]
    public void ToDisplayString_Scenarios(int min, int sec, int msec, string expectedString)
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(min)
            .Add(TimeSpan.FromSeconds(sec))
            .Add(TimeSpan.FromMilliseconds(msec));

        // Act
        var result = timeSpan.ToDisplayString();

        // Verify
        Assert.Equal(expectedString, result);
    }
}
