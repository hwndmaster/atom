using System.ComponentModel;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderTextColumn : IAutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn>
{
    IAutoGridContextBuilderTextColumn Filterable(bool filterable = true);
    IAutoGridContextBuilderTextColumn IsGrouped(bool isGrouped = true);
    IAutoGridContextBuilderTextColumn WithDisplayFormat(string displayFormat);
    IAutoGridContextBuilderTextColumn WithIconSource(IconSourceRecord iconSource);

    /// <summary>
    ///   Extends the text block of the cell to be able to highlight the text using the references pattern.
    /// </summary>
    /// <param name="patternPath">The path to the property of the parent view model which contains a pattern value.</param>
    /// <param name="useRegexPath">The path to the property of the parent view model which contains a boolean value indicating whether the pattern is a regular expression or not.</param>
    IAutoGridContextBuilderTextColumn WithTextHighlighting(string patternPath, string? useRegexPath = null);
}

internal sealed class AutoGridContextBuilderTextColumn : AutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn>, IAutoGridContextBuilderTextColumn
{
    private string? _displayFormat;
    private bool _filterable;
    private bool _isGrouped;
    private IconSourceRecord? _iconSource;
    private string? _textHighlightingPatternPath;
    private string? _textHighlightingUseRegexPath;

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

    public IAutoGridContextBuilderTextColumn WithTextHighlighting(string patternPath, string? useRegexPath = null)
    {
        _textHighlightingPatternPath = patternPath;
        _textHighlightingUseRegexPath = useRegexPath;
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
            TextHighlightingPatternPath = _textHighlightingPatternPath,
            TextHighlightingUseRegexPath = _textHighlightingUseRegexPath,
            ToolTip = _toolTip,
            ToolTipPath = _toolTipPath,
            ValueConverter = DetermineValueConverter(_displayFormat),
            Visibility = _visibilityBinding,
        };
    }

    protected override AutoGridContextBuilderTextColumn BuilderInstance => this;
}
