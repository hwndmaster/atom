using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Genius.Atom.Web;

/// <summary>
/// Provides helper methods for converting <see cref="ValidationResult"/> instances into API-friendly responses.
/// </summary>
internal static class ValidationResultExtensions
{
    /// <summary>
    /// Converts a collection of <see cref="ValidationResult"/> instances into a <see cref="ValidationProblemDetails"/> response payload.
    /// </summary>
    /// <param name="validationResults">The validation results to convert.</param>
    /// <returns>A <see cref="ValidationProblemDetails"/> containing the validation errors grouped by member name.</returns>
    public static ValidationProblemDetails ToValidationProblem(this IEnumerable<ValidationResult> validationResults)
    {
        return new ValidationProblemDetails(
            validationResults
                .Where(x => x.ErrorMessage is not null)
                .ToLookup(x => x.MemberNames.First(), x => x.ErrorMessage!)
                .ToDictionary(x => x.Key, x => x.ToArray()));
    }
}
