using Genius.Atom.UI.Forms.Demo.ViewModels;
using MahApps.Metro.Controls;

namespace Genius.Atom.UI.Forms.Demo.Views
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(MainViewModel mainVm)
        {
            InitializeComponent();

            DataContext = mainVm;
        }
    }
}
