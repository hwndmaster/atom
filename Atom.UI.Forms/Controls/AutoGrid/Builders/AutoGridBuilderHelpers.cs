using System.ComponentModel;
using System.Windows.Input;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;

internal static class AutoGridBuilderHelpers
{
    internal static IAutoGridContextBuilderColumn CreateContextBuilderColumn<TViewModel, TParentViewModel>(PropertyDescriptor property)
    {
        Guard.NotNull(property);

        if (IsCommandColumn(property))
        {
            return new AutoGridContextBuilderCommandColumn<TViewModel, TParentViewModel>(property);
        }

        if (IsGroupableColumn(property))
        {
            return new AutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>(property)
                .IsGrouped();
        }

        return new AutoGridContextBuilderTextColumn<TViewModel, TParentViewModel>(property);
    }

    internal static bool IsIgnorableProperty(string propertyName)
    {
        var ignoreProperties = new [] {
            nameof(IHasDirtyFlag.IsDirty),
            nameof(ISelectable.IsSelected),
            nameof(IEditable.IsEditing),
            nameof(INotifyDataErrorInfo.HasErrors)
        };

        return ignoreProperties.Contains(propertyName);
    }

    internal static bool IsCommandColumn(PropertyDescriptor property)
    {
        return typeof(ICommand).IsAssignableFrom(property.PropertyType);
    }

    internal static bool IsGroupableColumn(PropertyDescriptor property)
    {
        return typeof(IGroupableViewModel).IsAssignableFrom(property.PropertyType);
    }
}
