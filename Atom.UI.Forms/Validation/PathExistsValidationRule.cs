using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class PathExistsValidationRule : ValidationRule, IPropertyValidationRule
{
    public PathExistsValidationRule(string propertyName)
    {
        PropertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return ValidationResult.ValidResult;
        }

        if (!Path.Exists(value.ToString()))
        {
            return new ValidationResult(false, $"Path specified in {PropertyName} doesn't exist.");
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
