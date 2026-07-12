using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Genius.Atom.Web;

/// <summary>
/// Provides helper methods for converting <see cref="ValidationResult"/> instances into API-friendly responses.
/// </summary>
internal static class ValidationResultExtensions
{
    extension(IEnumerable<ValidationResult> validationResults)
    {
        /// <summary>
        /// Converts a collection of <see cref="ValidationResult"/> instances into a <see cref="ValidationProblem"/> response
        /// containing the validation errors grouped by member name.
        /// </summary>
        /// <returns>A <see cref="ValidationProblem"/> result producing an HTTP 400 with the validation errors.</returns>
        public ValidationProblem ToValidationProblem()
        {
            return TypedResults.ValidationProblem(
                validationResults
                    .Where(x => x.ErrorMessage is not null)
                    .SelectMany(x => x.MemberNames.Select(member => new { member, x.ErrorMessage }))
                    .ToLookup(x => x.member ?? string.Empty, x => x.ErrorMessage!)
                    .ToDictionary(x => x.Key, x => x.ToArray()));
        }
    }
}
