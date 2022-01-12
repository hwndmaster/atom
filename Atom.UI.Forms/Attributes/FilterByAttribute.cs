namespace Genius.Atom.UI.Forms;

/// <summary>
///   Indicates whether the property column may be filtered out in an auto-generated data grid.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FilterByAttribute : Attribute
{ }
