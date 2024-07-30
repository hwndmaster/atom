using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class PathExistsValidationRule : ValidationRule, IPropertyValidationRule
{
    private readonly bool _acceptMultiplePaths;

    /// <summary>
    ///   Initializes a new instance of <see cref="PathExistsValidationRule"/>.
    /// </summary>
    /// <param name="propertyName">The property name to be validated.</param>
    /// <param name="acceptMultiplePaths">Indicates whether multiple paths separated by comma (,) are supported.</param>
    public PathExistsValidationRule(string propertyName, bool acceptMultiplePaths = false)
    {
        PropertyName = propertyName;
        _acceptMultiplePaths = acceptMultiplePaths;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null)
        {
            return ValidationResult.ValidResult;
        }

        var paths = _acceptMultiplePaths
            ? value.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            : [value.ToString() ?? string.Empty];

        foreach (var path in paths)
        {
            if (!Path.Exists(path))
            {
                return new ValidationResult(false, $"Path specified in {PropertyName} doesn't exist.");
            }
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
