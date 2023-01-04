using System.Globalization;
using Genius.Atom.UI.Forms.Validation;

namespace Genius.Atom.UI.Forms.Tests.Validation;

public sealed class ValueRangeValidationRuleTests
{
    [Fact]
    public void TestDateTime_Positive()
    {
        // Arrange
        var sut = new ValueRangeValidationRule<DateTime>(() => new DateTime(1900, 1, 1), () => new DateTime(1900, 1, 2));

        // Act
        var result = sut.Validate(null!, CultureInfo.InvariantCulture);

        // Verify
        Assert.True(result.IsValid);
    }

    [Fact]
    public void TestDateTime_Negative()
    {
        // Arrange
        var sut = new ValueRangeValidationRule<DateTime>(() => new DateTime(1900, 1, 2), () => new DateTime(1900, 1, 1));

        // Act
        var result = sut.Validate(null!, CultureInfo.InvariantCulture);

        // Verify
        Assert.False(result.IsValid);
    }

    [Fact]
    public void TestInteger_Positive()
    {
        // Arrange
        var sut = new ValueRangeValidationRule<int>(() => 1, () => 5);

        // Act
        var result = sut.Validate(null!, CultureInfo.InvariantCulture);

        // Verify
        Assert.True(result.IsValid);
    }

    [Fact]
    public void TestInteger_Negative()
    {
        // Arrange
        var sut = new ValueRangeValidationRule<int>(() => 5, () => 1);

        // Act
        var result = sut.Validate(null!, CultureInfo.InvariantCulture);

        // Verify
        Assert.False(result.IsValid);
    }
}
