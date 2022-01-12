namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defined the selected property to render a static icon from resources in its UI content.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IconAttribute : Attribute
{
    public IconAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
