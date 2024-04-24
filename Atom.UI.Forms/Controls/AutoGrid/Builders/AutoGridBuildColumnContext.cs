using System.ComponentModel;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal abstract class AutoGridBuildColumnContext
{
    protected AutoGridBuildColumnContext(PropertyDescriptor property, AutoGridContextBuilderBaseFields baseFields)
    {
        Property = property;
        DisplayName = baseFields.DisplayName ?? property.Name;
        AutoWidth = baseFields.AutoWidth;
        IsReadOnly = baseFields.IsReadOnly;
        ToolTip = baseFields.ToolTip;
        ToolTipPath = baseFields.ToolTipPath;
        Style = baseFields.Style;
        ValueConverter = baseFields.ValueConverter;
        Visibility = baseFields.VisibilityBinding;
    }

    public bool IsGroupedColumn()
        => this is AutoGridBuildTextColumnContext textColumnContext
            && textColumnContext.IsGrouped;

    public PropertyDescriptor Property { get; }
    public string DisplayName { get; internal set; }
    public int? DisplayIndex { get; internal set; }
    public bool AutoWidth { get; }
    public bool IsReadOnly { get; }
    public string? ToolTip { get; }
    public string? ToolTipPath { get; }
    public StylingRecord? Style { get; }
    public IValueConverter? ValueConverter { get; }
    public string? Visibility { get; }

    public virtual bool IsAlwaysHidden => false;
}
