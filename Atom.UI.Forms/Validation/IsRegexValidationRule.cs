using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class IsRegexValidationRule : ValidationRule
{
    private readonly string? _propertyName;

    public IsRegexValidationRule(string? propertyName = null)
    {
        _propertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is string stringValue)
        {
            try
            {
                Regex.Match("", stringValue);
            }
            catch (ArgumentException)
            {
                return new ValidationResult(false, $"{_propertyName ?? "Value"} has invalid regular expression.");
            }
        }

        return ValidationResult.ValidResult;
    }
}
