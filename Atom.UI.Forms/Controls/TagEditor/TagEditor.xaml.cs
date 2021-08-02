using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Input;

namespace Genius.Atom.UI.Forms.Controls.TagEditor
{
    [ExcludeFromCodeCoverage]
    public partial class TagEditor
    {
        public TagEditor(TagEditorViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        public TagEditorViewModel ViewModel => (TagEditorViewModel) DataContext;

        private void AutoCompleteBox_KeyUp(object sender, KeyEventArgs e)
        {
            var acb = (AutoCompleteBox)sender;
            if (e.Key == Key.Enter)
            {
                ViewModel.AddSelected(acb.SelectedItem as TagItemViewModel, acb.Text.Trim());
                acb.SelectedItem = null;
                acb.Text = string.Empty;
            }
        }
    }
}
