namespace Genius.Atom.UI.Forms;

/// <summary>
///   Indicates whether the property control wants to occupy as much space as possible.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class GreedyAttribute : Attribute
{
}
