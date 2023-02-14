using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Wpf;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderColumn { }

public interface IAutoGridContextBuilderColumn<TBuilder> : IAutoGridContextBuilderColumn
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder>
{
    TBuilder IsReadOnly(bool isReadOnly = true);
    TBuilder WithAutoWidth(bool autoWidth = true);
    TBuilder WithDisplayName(string displayName);
    TBuilder WithStyle(StylingRecord style);
    TBuilder WithToolTip(string toolTip);
    TBuilder WithToolTipPath(string toolTipPath);
    TBuilder WithValueConverter<TValueConverter>()
        where TValueConverter : IValueConverter;
    TBuilder WithValueConverter(IValueConverter valueConverter);
    TBuilder WithValueConverter(Func<IValueConverter> valueConverterFactory);
}

internal abstract partial class AutoGridContextBuilderColumn<TBuilder> : IAutoGridContextBuilderColumn<TBuilder>, IHasBuildColumnContext
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder>
{
    private string? _displayName;
    protected bool _autoWidth = false;
    protected bool _isReadOnly = false;
    protected StylingRecord? _style;
    protected string? _toolTip;
    protected string? _toolTipPath;
    protected IValueConverter? _valueConverter;
    protected Func<IValueConverter>? _valueConverterFactory;
    protected Type? _valueConverterType;

    protected AutoGridContextBuilderColumn(PropertyDescriptor propertyDescriptor)
    {
        PropertyDescriptor = propertyDescriptor;
    }

    public TBuilder IsReadOnly(bool isReadOnly = true)
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

    public TBuilder WithToolTipPath(string toolTipPath)
    {
        _toolTipPath = toolTipPath;
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

    public abstract AutoGridBuildColumnContext Build();

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

        if (_valueConverterType is null && PropertyDescriptor.PropertyType.IsValueType)
        {
            return new PropertyValueStringConverter(displayFormat);
        }

        return null;
    }

    protected abstract TBuilder BuilderInstance { get; }
    protected PropertyDescriptor PropertyDescriptor { get; }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex DisplayNameRegex();
}
