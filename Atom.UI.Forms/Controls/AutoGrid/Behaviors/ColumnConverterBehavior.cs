using System;
using System.Windows.Data;
using Genius.Atom.UI.Forms.Attributes;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    public class ColumnConverterBehavior : IAutoGridColumnBehavior
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
