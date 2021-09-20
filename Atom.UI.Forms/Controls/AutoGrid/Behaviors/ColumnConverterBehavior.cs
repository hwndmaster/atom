using System;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    internal class ColumnConverterBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            var converterAttr = context.GetAttribute<ValueConverterAttribute>();
            if (converterAttr == null)
            {
                return;
            }

            var binding = context.GetBinding();
            binding.Converter = (IValueConverter)Activator.CreateInstance(converterAttr.ValueConverterType);
        }
    }
}
