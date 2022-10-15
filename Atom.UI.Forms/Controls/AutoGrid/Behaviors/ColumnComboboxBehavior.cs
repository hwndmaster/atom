using System.Collections;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors;

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
            context.Args.Column = WpfBuilders.DataGridColumnBuilder
                .ForValuePath(context.Property.Name)
                .WithComboEditor(comboBoxContext.CollectionPropertyName)
                .WithImageSource(
                    context.Property.PropertyType == typeof(ITitledItemWithImageViewModel)
                        ? $"{context.Property.Name}.{nameof(ITitledItemWithImageViewModel.Image)}"
                        : null
                )
                .Build();
        }
    }
}
