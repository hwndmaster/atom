using System;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    internal class ColumnConverterBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            var binding = context.GetBinding();
            if (binding == null)
            {
                return;
            }

            var converterAttr = context.GetAttribute<ValueConverterAttribute>();
            if (converterAttr != null)
            {
                binding.Converter = (IValueConverter)Activator.CreateInstance(converterAttr.ValueConverterType);
            }

            if (binding.Converter == null)
            {
                binding.Converter = new PropertyValueStringConverter();
            }
        }
    }
}
