namespace Genius.Atom.UI.Forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TitleAttribute : Attribute
{
    public TitleAttribute(string title)
    {
        Title = title;
    }

    public string Title { get; }
}
