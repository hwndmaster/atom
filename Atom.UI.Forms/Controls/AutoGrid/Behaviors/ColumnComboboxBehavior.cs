using System.Collections;
using Genius.Atom.UI.Forms.Attributes;
using Genius.Atom.UI.Forms.ViewModels;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Behaviors
{
    public class ColumnComboboxBehavior : IAutoGridColumnBehavior
    {
        public void Attach(AutoGridColumnContext context)
        {
            if (context.DataGrid.IsReadOnly || context.Args.Column.IsReadOnly)
            {
                return;
            }

            var selectFromListAttr = context.GetAttribute<SelectFromListAttribute>();
            if (selectFromListAttr == null)
            {
                return;
            }

            if (selectFromListAttr.FromOwnerContext)
            {
                var prop = context.DataGrid.DataContext.GetType().GetProperty(selectFromListAttr.CollectionPropertyName);
                var value = prop.GetValue(context.DataGrid.DataContext) as IEnumerable;
                context.Args.Column = WpfHelpers.CreateComboboxColumnWithStaticItemsSource(
                    value, context.Property.Name);
            }
            else
            {
                context.Args.Column = WpfBuilders.DataGridColumnBuilder
                    .ForValuePath(context.Property.Name)
                    .WithComboEditor(selectFromListAttr.CollectionPropertyName)
                    .WithImageSource(
                        context.Property.PropertyType == typeof(ITitledItemWithImageViewModel)
                            ? $"{context.Property.Name}.{nameof(ITitledItemWithImageViewModel.Image)}"
                            : null
                    )
                    .Build();
            }
        }
    }
}
