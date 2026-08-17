using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class IsRegexValidationRule : ValidationRule, IPropertyValidationRule
{
    public IsRegexValidationRule(string propertyName)
    {
        PropertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return ValidationResult.ValidResult;
        }

        if (value is string stringValue)
        {
            try
            {
                // The input parameter doesn't matter, as we're trying to validate the regular expression itself.
                Regex.Match("", stringValue, RegexOptions.None, TimeSpan.FromSeconds(10));
            }
            catch (ArgumentException)
            {
                return new ValidationResult(false, $"{PropertyName} has invalid regular expression.");
            }
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
