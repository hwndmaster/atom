using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Genius.Atom.UI.Forms;

public abstract class MarkupFuncConverterBase<TSource, TTarget> : MarkupExtension, IValueConverter
{
    protected MarkupFuncConverterBase(Func<TSource, bool> valueChecker, TSource sourceTrue, TSource sourceFalse, TTarget targetTrue, TTarget targetFalse)
    {
        ValueChecker = valueChecker.NotNull();
        SourceTrue = sourceTrue.NotNull();
        SourceFalse = sourceFalse.NotNull();
        TargetTrue = targetTrue.NotNull();
        TargetFalse = targetFalse.NotNull();
    }

    public Func<TSource, bool> ValueChecker { get; set; }
    public TSource SourceTrue { get; set; }
    public TSource SourceFalse { get; set; }
    public TTarget TargetTrue { get; set; }
    public TTarget TargetFalse { get; set; }

    public virtual object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TSource valueTyped)
        {
            return ValueChecker(valueTyped) ? TargetTrue : TargetFalse;
        }

        return DependencyProperty.UnsetValue;
    }

    public virtual object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TTarget valueAsT)
        {
            return EqualityComparer<TTarget>.Default.Equals(valueAsT, TargetTrue)
                ? SourceTrue
                : SourceFalse;
        }

        return DependencyProperty.UnsetValue;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}
