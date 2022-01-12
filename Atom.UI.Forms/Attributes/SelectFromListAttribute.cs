namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines the binding path to the collection of items to be displayed in the
///   dropdown list in an auto-generated data grid.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SelectFromListAttribute : Attribute
{
    public SelectFromListAttribute(string collectionPropertyName, bool fromOwnerContext = false)
    {
        CollectionPropertyName = collectionPropertyName;
        FromOwnerContext = fromOwnerContext;
    }

    public string CollectionPropertyName { get; }
    public bool FromOwnerContext { get; }
}
