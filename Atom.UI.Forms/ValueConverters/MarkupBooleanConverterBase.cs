using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Genius.Atom.UI.Forms
{
    public abstract class MarkupBooleanConverterBase<T> : MarkupExtension, IValueConverter
    {
        protected MarkupBooleanConverterBase(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool valueAsBool && valueAsBool ? True : False;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T valueAsT && EqualityComparer<T>.Default.Equals(valueAsT, True);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
