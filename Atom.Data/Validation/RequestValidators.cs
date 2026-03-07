using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Genius.Atom.Data.Validation;
internal sealed class RequestValidators : IRequestValidators
{
    private readonly IEnumerable<IRequestValidator> _validators;

    public RequestValidators(IEnumerable<IRequestValidator> validators)
    {
        _validators = validators;
    }

    public IEnumerable<IRequestValidator<T>> GetValidators<T>()
    {
        foreach (var validator in _validators)
        {
            if (validator is IRequestValidator<T> typedValidator)
            {
                yield return typedValidator;
            }
        }
    }

    public async IAsyncEnumerable<ValidationResult> ValidateAsync<T>(T request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var validator in GetValidators<T>())
        {
            ValidationResult? result = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (result is not null && result != ValidationResult.Success)
            {
                yield return result;
            }
        }
    }
}
