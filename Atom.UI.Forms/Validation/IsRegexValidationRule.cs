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
                Regex.Match("", stringValue);
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
