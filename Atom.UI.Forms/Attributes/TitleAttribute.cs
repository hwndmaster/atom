namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a property title to be diplayed in the UI instead
///   of auto-generated one based on the property name.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class TitleAttribute : Attribute
{
    public TitleAttribute(string title)
    {
        Title = title;
    }

    public string Title { get; }
}
