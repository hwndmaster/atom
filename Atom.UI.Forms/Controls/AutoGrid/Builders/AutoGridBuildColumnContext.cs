using System.ComponentModel;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal abstract class AutoGridBuildColumnContext
{
    protected AutoGridBuildColumnContext(PropertyDescriptor property, string? displayName = null)
    {
        Property = property;
        DisplayName = displayName ?? property.Name;
        AutoWidth = false;
        IsReadOnly = false;
    }

    public bool IsGroupedColumn()
        => this is AutoGridBuildTextColumnContext textColumnContext
            && textColumnContext.IsGrouped;

    public PropertyDescriptor Property { get; }
    public string DisplayName { get; internal set; }
    public int? DisplayIndex { get; internal set; }
    public bool AutoWidth { get; init; }
    public bool IsReadOnly { get; init; }
    public string? ToolTip { get; init; }
    public string? ToolTipPath { get; init; }
    public StylingRecord? Style { get; init; }
    public IValueConverter? ValueConverter { get; init; }
    public string? Visibility { get; init; }
}
