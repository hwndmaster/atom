using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

public interface IAutoGridContextBuilderColumn { }

public interface IAutoGridContextBuilderColumn<TBuilder> : IAutoGridContextBuilderColumn
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder>
{
    TBuilder IsReadOnly(bool isReadOnly = true);
    TBuilder WithAutoWidth(bool autoWidth = true);
    TBuilder WithDisplayName(string displayName);
    TBuilder WithStyle(StylingRecord style);
    TBuilder WithToolTipPath(string toolTipPath);
    TBuilder WithValueConverter<TValueConverter>()
        where TValueConverter : IValueConverter;
}

internal abstract class AutoGridContextBuilderColumn<TBuilder> : IAutoGridContextBuilderColumn<TBuilder>, IHasBuildColumnContext
    where TBuilder : IAutoGridContextBuilderColumn<TBuilder>
{
    private string? _displayName;
    protected bool _autoWidth = false;
    protected bool _isReadOnly = false;
    protected StylingRecord? _style;
    protected string? _toolTipPath;
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

    public abstract AutoGridBuildColumnContext Build();

    protected string DetermineDisplayName()
    {
        return _displayName
            ?? Regex.Replace(PropertyDescriptor.Name, "[A-Z]", " $0");
    }

    protected IValueConverter? DetermineValueConverter(string? displayFormat)
    {
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
}
