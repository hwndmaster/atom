namespace Genius.Atom.UI.Forms;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class AttachedViewAttribute : Attribute
{
    public AttachedViewAttribute(Type attachedViewType)
    {
        AttachedViewType = attachedViewType;
    }

    public Type AttachedViewType { get; }
}
