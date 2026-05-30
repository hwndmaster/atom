using System.Windows.Controls;

namespace Genius.Atom.UI.Forms.Controls.AutoGrid;

internal static class AutoGridExtensions
{
    extension(DataGrid dataGrid)
    {
        public ViewModelBase GetViewModel()
        {
            return dataGrid.DataContext as ViewModelBase
                ?? throw new InvalidCastException($"Cannot cast DataContext to {nameof(ViewModelBase)}");
        }

        public BindingProxy GetBindingProxy()
        {
            return (BindingProxy)dataGrid.FindResource("proxy").NotNull();
        }
    }
}
