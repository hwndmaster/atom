namespace Genius.Atom.UI.Forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TooltipSourceAttribute : Attribute
{
    public TooltipSourceAttribute(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
