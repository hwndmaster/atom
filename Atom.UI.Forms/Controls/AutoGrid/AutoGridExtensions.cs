using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal static class AutoGridExtensions
{
    public static ViewModelBase GetViewModel(this DataGrid dataGrid)
    {
        return dataGrid.DataContext as ViewModelBase
            ?? throw new InvalidCastException($"Cannot cast DataContext to {nameof(ViewModelBase)}");
    }

    public static BindingProxy GetBindingProxy(this DataGrid dataGrid)
    {
        return (BindingProxy)dataGrid.FindResource("proxy").NotNull();
    }
}
