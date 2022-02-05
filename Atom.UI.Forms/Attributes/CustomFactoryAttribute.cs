namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a factory type to resolve when creating a new item in the auto-generated data grids.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CustomFactoryAttribute : Attribute
{
    public CustomFactoryAttribute(Type factoryType, string createMethod = "Create")
    {
        FactoryType = factoryType;
        CreateMethod = createMethod;
    }

    public Type FactoryType { get; }
    public string CreateMethod { get; }
}
