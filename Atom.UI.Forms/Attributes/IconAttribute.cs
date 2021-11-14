namespace Genius.Atom.UI.Forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IconAttribute : Attribute
{
    public IconAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
