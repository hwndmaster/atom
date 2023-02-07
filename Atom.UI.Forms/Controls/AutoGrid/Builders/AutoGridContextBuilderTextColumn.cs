using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderTextColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn>
{
    IAutoGridContextBuilderTextColumn Filterable(bool filterable = true);
    IAutoGridContextBuilderTextColumn IsGrouped(bool isGrouped = true);
    IAutoGridContextBuilderTextColumn WithDisplayFormat(string displayFormat);
    IAutoGridContextBuilderTextColumn WithIconSource(IconSourceRecord iconSource);
}

internal sealed class AutoGridContextBuilderTextColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn>, IAutoGridContextBuilderTextColumn
{
    private string? _displayFormat;
    private bool _filterable;
    private bool _isGrouped;
    private IconSourceRecord? _iconSource;

    public AutoGridContextBuilderTextColumn(PropertyDescriptor propertyDescriptor)
        : base(propertyDescriptor)
    {
    }

    public IAutoGridContextBuilderTextColumn Filterable(bool filterable = true)
    {
        _filterable = filterable;
        return this;
    }

    public IAutoGridContextBuilderTextColumn IsGrouped(bool isGrouped = true)
    {
        _isGrouped = isGrouped;
        return this;
    }

    public IAutoGridContextBuilderTextColumn WithDisplayFormat(string displayFormat)
    {
        _displayFormat = displayFormat;
        return this;
    }

    public IAutoGridContextBuilderTextColumn WithIconSource(IconSourceRecord iconSource)
    {
        _iconSource = iconSource;
        return this;
    }

    public override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildTextColumnContext(PropertyDescriptor, DetermineDisplayName())
        {
            AutoWidth = _autoWidth,
            DisplayFormat = _displayFormat,
            Filterable = _filterable,
            IsGrouped = _isGrouped,
            IsReadOnly = _isReadOnly,
            IconSource = _iconSource,
            Style = _style,
            ToolTip = _toolTip,
            ToolTipPath = _toolTipPath,
            ValueConverter = DetermineValueConverter(_displayFormat)
        };
    }

    protected override AutoGridContextBuilderTextColumn BuilderInstance => this;
}
