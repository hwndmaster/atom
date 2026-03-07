using System.ComponentModel.DataAnnotations;

namespace Genius.Atom.Data.Validation;

/// <summary>
/// A non-generic marker interface for request validators.
/// </summary>
public interface IRequestValidator
{
}

/// <summary>
/// Defines a contract for validating requests of a specific type.
/// </summary>
/// <typeparam name="T">The type of the request to validate.</typeparam>
public interface IRequestValidator<in T> : IRequestValidator
{
    /// <summary>
    /// Validates the specified request and returns the result of the validation.
    /// </summary>
    /// <param name="request">The request object to validate.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    ///   A <see cref="ValidationResult"/> representing the outcome of the validation.
    ///   The result indicates whether the request is valid and may include validation errors if applicable.
    /// </returns>
    Task<ValidationResult?> ValidateAsync(T request, CancellationToken cancellationToken = default);
}
