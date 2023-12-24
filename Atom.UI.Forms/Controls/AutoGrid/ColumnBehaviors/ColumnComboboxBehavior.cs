using System.Collections;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Wpf;
using Genius.Atom.UI.Forms.Wpf.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.ColumnBehaviors;

internal sealed class ColumnComboBoxBehavior : IAutoGridColumnBehavior
{
    public void Attach(AutoGridColumnContext context)
    {
        if (context.IsReadOnly)
        {
            return;
        }

        if (context.BuildColumn is not AutoGridBuildComboBoxColumnContext comboBoxContext)
        {
            return;
        }

        if (comboBoxContext.FromOwnerContext)
        {
            var prop = context.DataGrid.DataContext.GetType().GetProperty(comboBoxContext.CollectionPropertyName).NotNull();
            var value = (IEnumerable)prop.GetValue(context.DataGrid.DataContext).NotNull();
            context.Args.Column = WpfHelpers.CreateComboboxColumnWithStaticItemsSource(
                value, context.Property.Name);
        }
        else
        {
            var builder = DataGridColumnBuilder
                .ForValuePath(context.Property.Name)
                .BasedOnAutoGridColumnContext(context)
                .WithComboEditor(comboBoxContext.CollectionPropertyName);

            if (context.Property.PropertyType == typeof(ITitledItemWithImageViewModel))
            {
                builder = builder.RenderAsTextWithImage($"{context.Property.Name}.{nameof(ITitledItemWithImageViewModel.Image)}");
            }

            context.Args.Column = builder.Build();
        }
    }
}
