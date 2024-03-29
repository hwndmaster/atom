using System.Globalization;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class NotNullValidationRule : ValidationRule, IPropertyValidationRule
{
    public NotNullValidationRule(string propertyName)
    {
        PropertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return new ValidationResult(false, $"{PropertyName} cannot be null.");
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
