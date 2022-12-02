using System.Globalization;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class StringNotNullOrEmptyValidationRule : ValidationRule
{
    private readonly string? _propertyName;

    public StringNotNullOrEmptyValidationRule(string? propertyName = null)
    {
        _propertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string stringValue)
        {
            return new ValidationResult(false, $"{_propertyName ?? "Value"} has a non-string value.");
        }

        if (string.IsNullOrEmpty(stringValue))
        {
            return new ValidationResult(false, $"{_propertyName ?? "Value"} cannot be empty.");
        }

        return ValidationResult.ValidResult;
    }
}
