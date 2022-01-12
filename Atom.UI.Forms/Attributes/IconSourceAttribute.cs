namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines the binding path to be used to render an icon in front of
///   the cell content in an auto-generated data grid.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IconSourceAttribute : Attribute
{
    public IconSourceAttribute(string iconPropertyPath)
    {
        IconPropertyPath = iconPropertyPath;
    }

    public IconSourceAttribute(string iconPropertyPath, double fixedSize)
        : this (iconPropertyPath)
    {
        FixedSize = fixedSize;
    }

    public IconSourceAttribute(string iconPropertyPath, double fixedSize, bool hideText)
        : this (iconPropertyPath, fixedSize)
    {
        HideText = hideText;
    }

    public string IconPropertyPath { get; }
    public double? FixedSize { get; }
    public bool HideText { get; } = false;
}
