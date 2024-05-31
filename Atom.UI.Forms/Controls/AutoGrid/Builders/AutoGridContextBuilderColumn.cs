using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderColumn { }

public interface IAutoGridContextBuilderColumn<TBuilder, TViewModel, TParentViewModel> : IAutoGridContextBuilderColumn
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder, TViewModel, TParentViewModel>
{
    TBuilder IsReadOnly(bool isReadOnly = true);
    TBuilder WithAutoWidth(bool autoWidth = true);
    TBuilder WithDisplayName(string displayName);
    TBuilder WithStyle(StylingRecord style);
    TBuilder WithToolTip(string toolTip);
    TBuilder WithToolTipPath(Expression<Func<TViewModel, string>> toolTipPath);
    TBuilder WithValueConverter<TValueConverter>()
        where TValueConverter : IValueConverter;
    TBuilder WithValueConverter(IValueConverter valueConverter);
    TBuilder WithValueConverter(Func<IValueConverter> valueConverterFactory);

    /// <summary>
    ///   Extends the column to make it hidable depending on the value, provided by the path at <paramref name="visibilityProperty"/>.
    /// </summary>
    /// <param name="visibilityProperty">The property of the parent view model which contains a boolean value indicating whether the column must be visible or not.</param>
    TBuilder WithVisibility(Expression<Func<TParentViewModel, bool>> visibilityProperty);
}

internal abstract partial class AutoGridContextBuilderColumn<TBuilder, TVIewModel, TParentViewModel>
    : IAutoGridContextBuilderColumn<TBuilder, TVIewModel, TParentViewModel>,
        IHasBuildColumnContext
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder, TVIewModel, TParentViewModel>
{
    private string? _displayName;
    protected bool _autoWidth;
    protected bool _isReadOnly;
    protected StylingRecord? _style;
    protected string? _toolTip;
    protected string? _toolTipPath;
    protected IValueConverter? _valueConverter;
    protected Func<IValueConverter>? _valueConverterFactory;
    protected Type? _valueConverterType;
    protected string? _visibilityBinding;

    protected AutoGridContextBuilderColumn(PropertyDescriptor propertyDescriptor)
    {
        PropertyDescriptor = propertyDescriptor;
    }

    public virtual TBuilder IsReadOnly(bool isReadOnly = true)
    {
        _isReadOnly = isReadOnly;
        return BuilderInstance;
    }

    public TBuilder WithAutoWidth(bool autoWidth = true)
    {
        _autoWidth = autoWidth;
        return BuilderInstance;
    }

    public TBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return BuilderInstance;
    }

    public TBuilder WithStyle(StylingRecord style)
    {
        _style = style;
        return BuilderInstance;
    }

    public TBuilder WithToolTip(string toolTip)
    {
        _toolTip = toolTip;
        return BuilderInstance;
    }

    public TBuilder WithToolTipPath(Expression<Func<TVIewModel, string>> toolTipPath)
    {
        _toolTipPath = ExpressionHelpers.GetPropertyName(toolTipPath);
        return BuilderInstance;
    }

    public TBuilder WithValueConverter<TValueConverter>()
        where TValueConverter : IValueConverter
    {
        _valueConverterType = typeof(TValueConverter);
        return BuilderInstance;
    }

    public TBuilder WithValueConverter(IValueConverter valueConverter)
    {
        _valueConverter = valueConverter;
        return BuilderInstance;
    }

    public TBuilder WithValueConverter(Func<IValueConverter> valueConverterFactory)
    {
        _valueConverterFactory = valueConverterFactory;
        return BuilderInstance;
    }

    public TBuilder WithVisibility(Expression<Func<TParentViewModel, bool>> visibilityProperty)
    {
        _visibilityBinding = ExpressionHelpers.GetPropertyName(visibilityProperty);
        return BuilderInstance;
    }

    internal abstract AutoGridBuildColumnContext Build();
    public AutoGridBuildColumnContext BuildInternal() => Build();

    protected string DetermineDisplayName()
    {
        return _displayName
            ?? DisplayNameRegex().Replace(PropertyDescriptor.Name, " $0");
    }

    protected IValueConverter? DetermineValueConverter(string? displayFormat)
    {
        if (_valueConverter is not null)
        {
            return _valueConverter;
        }

        if (_valueConverterFactory is not null)
        {
            return _valueConverterFactory();
        }

        if (_valueConverterType is not null)
        {
            return ((Module.ServiceProvider.GetService(_valueConverterType)
                    ?? Activator.CreateInstance(_valueConverterType)) as IValueConverter)
                .NotNull();
        }

        if (PropertyDescriptor.PropertyType.IsValueType)
        {
            return new PropertyValueStringConverter(displayFormat);
        }

        return null;
    }

    protected AutoGridContextBuilderBaseFields GetBaseFields(bool omitDisplayName = false, string? valueConverterDisplayFormat = null)
    {
        var displayName = omitDisplayName ? null : DetermineDisplayName();
        var valueConverter = DetermineValueConverter(valueConverterDisplayFormat);

        return new AutoGridContextBuilderBaseFields(_autoWidth, displayName, _isReadOnly, _style, _toolTip, _toolTipPath, valueConverter, _visibilityBinding);
    }

    protected abstract TBuilder BuilderInstance { get; }
    protected PropertyDescriptor PropertyDescriptor { get; }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex DisplayNameRegex();
}
