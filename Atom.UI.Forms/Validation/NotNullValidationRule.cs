using System.Globalization;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class NotNullValidationRule : ValidationRule
{
    private readonly string? _propertyName;

    public NotNullValidationRule(string? propertyName = null)
    {
        _propertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return new ValidationResult(false, $"{_propertyName ?? "Value"} cannot be null.");
        }

        return ValidationResult.ValidResult;
    }
}
