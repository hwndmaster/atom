using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Genius.Atom.Web;

/// <summary>
/// Provides helper methods for converting <see cref="ValidationResult"/> instances into API-friendly responses.
/// </summary>
internal static class ValidationResultExtensions
{
    extension(IEnumerable<ValidationResult> validationResults)
    {
        /// <summary>
        /// Converts a collection of <see cref="ValidationResult"/> instances into a <see cref="ValidationProblemDetails"/> response payload.
        /// </summary>
        /// <returns>A <see cref="ValidationProblemDetails"/> containing the validation errors grouped by member name.</returns>
        public ValidationProblemDetails ToValidationProblem()
        {
            return new ValidationProblemDetails(
                validationResults
                    .Where(x => x.ErrorMessage is not null)
                    .SelectMany(x => x.MemberNames.Select(member => new { member, x.ErrorMessage }))
                    .ToLookup(x => x.member ?? string.Empty, x => x.ErrorMessage!)
                    .ToDictionary(x => x.Key, x => x.ToArray()));
        }
    }
}
