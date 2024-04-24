using System.ComponentModel;
using System.Linq.Expressions;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>
    : IAutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>
{
    IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> Filterable(bool filterable = true);
    IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> IsGrouped(bool isGrouped = true);
    IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithDisplayFormat(string displayFormat);
    IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithIconSource(IconSourceRecord<TViewModel> iconSource);

    /// <summary>
    ///   Extends the text block of the cell to be able to highlight the text using the references pattern.
    /// </summary>
    /// <param name="patternProperty">The property of the parent view model which contains a pattern value.</param>
    /// <param name="useRegexProperty">The property of the parent view model which contains a boolean value indicating whether the pattern is a regular expression or not.</param>
    IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithTextHighlighting(Expression<Func<TParentViewModel, string>> patternProperty, Expression<Func<TParentViewModel, bool>>? useRegexProperty = null);
}

internal sealed class AutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>
    : AutoGridContextBuilderColumn<IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>, TViewModel, TParentViewModel>, IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>
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

    public IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> Filterable(bool filterable = true)
    {
        _filterable = filterable;
        return this;
    }

    public IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> IsGrouped(bool isGrouped = true)
    {
        _isGrouped = isGrouped;
        return this;
    }

    public IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithDisplayFormat(string displayFormat)
    {
        _displayFormat = displayFormat;
        return this;
    }

    public IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithIconSource(IconSourceRecord<TViewModel> iconSource)
    {
        Guard.NotNull(iconSource);

        _iconSource = new IconSourceRecord(
            ExpressionHelpers.GetPropertyName(iconSource.IconPropertyPath),
            iconSource.FixedSize,
            iconSource.HideText);
        return this;
    }

    public IAutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> WithTextHighlighting(Expression<Func<TParentViewModel, string>> patternProperty, Expression<Func<TParentViewModel, bool>>? useRegexProperty = null)
    {
        _textHighlightingPatternPath = ExpressionHelpers.GetPropertyName(patternProperty);
        _textHighlightingUseRegexPath = useRegexProperty is null
            ? null
            : ExpressionHelpers.GetPropertyName(useRegexProperty);
        return this;
    }

    internal override AutoGridBuildColumnContext Build()
    {
        return new AutoGridBuildTextColumnContext(PropertyDescriptor, GetBaseFields(valueConverterDisplayFormat: _displayFormat))
        {
            DisplayFormat = _displayFormat,
            Filterable = _filterable,
            IsGrouped = _isGrouped,
            IconSource = _iconSource,
            TextHighlightingPatternPath = _textHighlightingPatternPath,
            TextHighlightingUseRegexPath = _textHighlightingUseRegexPath
        };
    }

    protected override AutoGridContextBuilderTextColumn<TViewModel, TParentViewModel> BuilderInstance => this;
}
