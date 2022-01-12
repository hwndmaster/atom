namespace Genius.Atom.UI.Forms;

/// <summary>
///   Tells the auto-generated data grid to only show columns for the properties
///   with the <see cref="System.ComponentModel.BrowsableAttribute"/> attribute defined.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ShowOnlyBrowsableAttribute : Attribute
{
    public ShowOnlyBrowsableAttribute(bool onlyBrowsable)
    {
        OnlyBrowsable = onlyBrowsable;
    }

    public bool OnlyBrowsable { get; }
}
