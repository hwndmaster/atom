namespace Genius.Atom.UI.Forms;

/// <summary>
///   Defines a user control type to be used in UI instead of default content.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AttachedViewAttribute : Attribute
{
    public AttachedViewAttribute(Type attachedViewType)
    {
        AttachedViewType = attachedViewType;
    }

    public Type AttachedViewType { get; }
}
