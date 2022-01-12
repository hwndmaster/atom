namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a value converter for the property control.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ValueConverterAttribute : Attribute
{
    public ValueConverterAttribute(Type valueConverterType)
    {
        ValueConverterType = valueConverterType;
    }

    public Type ValueConverterType { get; }
}
