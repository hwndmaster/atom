using System.ComponentModel;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public record StylingRecord(HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Left);

public abstract class AutoGridBuildColumnContext
{
    protected AutoGridBuildColumnContext(PropertyDescriptor property, string displayName)
    {
        Property = property;
        DisplayName = displayName;
        AutoWidth = false;
        IsReadOnly = false;
    }

    public PropertyDescriptor Property { get; }
    public string DisplayName { get; }
    public bool AutoWidth { get; init; }
    public bool IsReadOnly { get; init; }
    public string? ToolTipPath { get; init; }
    public StylingRecord? Style { get; init; }
    public IValueConverter? ValueConverter { get; init; }
}
