using System.Globalization;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class StringNotNullOrEmptyValidationRule : ValidationRule, IPropertyValidationRule
{
    public StringNotNullOrEmptyValidationRule(string propertyName)
    {
        PropertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is not string stringValue)
        {
            return new ValidationResult(false, $"{PropertyName} is not a string value.");
        }

        if (string.IsNullOrEmpty(stringValue))
        {
            return new ValidationResult(false, $"{PropertyName} cannot be empty.");
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
