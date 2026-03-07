using System.ComponentModel.DataAnnotations;
using Genius.Atom.Data.Validation;

namespace Genius.Atom.Data.Tests.Validation;

public class RequestValidatorsTests
{
    [Fact]
    public void GetValidators_ReturnsTypedValidatorsOnly()
    {
        // Arrange
        var validatorMock1 = new IRequestValidatorImposter<string>();
        var validatorMock2 = new IRequestValidatorImposter<int>();
        var validators = new IRequestValidator[] { validatorMock1.Instance(), validatorMock2.Instance() };
        var sut = new RequestValidators(validators);

        // Act
        var result = sut.GetValidators<string>().ToList();

        // Verify
        Assert.Single(result);
        Assert.Same(validatorMock1.Instance(), result[0]);
    }

    [Fact]
    public async Task ValidateAsync_YieldsValidationResults()
    {
        // Arrange
        var validatorMock = new IRequestValidatorImposter<string>();
        var expectedResult = new ValidationResult("Error");
        validatorMock.ValidateAsync("test", Arg<CancellationToken>.Any()).ReturnsAsync(expectedResult);
        var validators = new IRequestValidator[] { validatorMock.Instance() };
        var sut = new RequestValidators(validators);
        var results = new List<ValidationResult>();

        // Act
        await foreach (var result in sut.ValidateAsync("test", TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }

        // Verify
        Assert.Single(results);
        Assert.Same(expectedResult, results[0]);
    }

    [Fact]
    public async Task ValidateAsync_SkipsSuccessResults()
    {
        // Arrange
        var validatorMock = new IRequestValidatorImposter<string>();
        validatorMock.ValidateAsync("test", Arg<CancellationToken>.Any()).ReturnsAsync(ValidationResult.Success!);
        var validators = new IRequestValidator[] { validatorMock.Instance() };
        var sut = new RequestValidators(validators);
        var results = new List<ValidationResult>();

        // Act
        await foreach (var result in sut.ValidateAsync("test", TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }

        // Verify
        Assert.Empty(results);
    }
}
