namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines the binding path for the property control's tooltip.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TooltipSourceAttribute : Attribute
{
    public TooltipSourceAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
