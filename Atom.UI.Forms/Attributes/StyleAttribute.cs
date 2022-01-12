namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines additional styling properties for the property control.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class StyleAttribute : Attribute
{
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
}
