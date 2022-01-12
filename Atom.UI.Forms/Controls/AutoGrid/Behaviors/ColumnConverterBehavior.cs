using System.Windows.Data;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

internal sealed class ColumnConverterBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        var binding = context.GetBinding();
        if (binding is null)
        {
            return;
        }

        var converterAttr = context.GetAttribute<ValueConverterAttribute>();
        if (converterAttr is not null)
        {
            var instance = Activator.CreateInstance(converterAttr.ValueConverterType) as IValueConverter;
            binding.Converter = instance.NotNull(nameof(instance));
        }

        if (binding.Converter == null && !context.Args.PropertyType.IsValueType)
        {
            binding.Converter = new PropertyValueStringConverter();
        }
    }
}
