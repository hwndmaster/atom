using System.Globalization;
using System.Numerics;
using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Validation;

public sealed class NumberInRangeValidationRule<T> : ValidationRule, IPropertyValidationRule
    where T : INumber<T>
{
    private readonly T _from;
    private readonly T _to;

    public NumberInRangeValidationRule(T from, T @to, string propertyName)
    {
        _from = from;
        _to = @to;
        PropertyName = propertyName;
    }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is T number
            && (number < _from || number > _to))
        {
            return new ValidationResult(false, $"{PropertyName} must be in range [{_from}..{_to}].");
        }

        return ValidationResult.ValidResult;
    }

    public string PropertyName { get; }
}
