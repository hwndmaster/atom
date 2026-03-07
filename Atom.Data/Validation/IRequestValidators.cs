using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace Genius.Atom.Data.Validation;

/// <summary>
/// Defines a contract for retrieving a collection of validators for a specific request type.
/// </summary>
public interface IRequestValidators
{
    /// <summary>
    ///   Retrieves a collection of validators for the specified type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the object to be validated.</typeparam>
    /// <returns>
    ///   An enumerable collection of <see cref="IRequestValidator{T}"/> instances that can validate objects of type
    ///   <typeparamref name="TRequest"/>. If no validators are available, the collection will be empty.
    /// </returns>
    IEnumerable<IRequestValidator<TRequest>> GetValidators<TRequest>();

    /// <summary>
    ///   Validates the specified request and returns a collection of validation results.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="request">The request object to validate.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    ///   An asynchronous enumerable of <see cref="ValidationResult"/> objects representing the
    ///   validation results. The collection will be empty if the request is valid.
    /// </returns>
    IAsyncEnumerable<ValidationResult> ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default);
}
