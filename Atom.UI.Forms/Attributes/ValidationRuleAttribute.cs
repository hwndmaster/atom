namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a validation rule for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValidationRuleAttribute : Attribute
{
    public ValidationRuleAttribute(Type validationRuleType, params object[] parameters)
    {
        ValidationRuleType = validationRuleType;
        Parameters = parameters;
    }

    public Type ValidationRuleType { get; }
    public object[] Parameters { get; }
}
