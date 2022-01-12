namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a display index for a property column used in auto-generated data grids.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class DisplayIndexAttribute : Attribute
{
    public DisplayIndexAttribute(int index)
    {
        Index = index;
    }

    public int Index { get; }
}
