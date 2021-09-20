using System.Collections.ObjectModel;

namespace Genius.Atom.UI.Forms.Controls.TagEditor
{
    public interface ITagEditorViewModel
    {
        void AddSelected(ITagItemViewModel tagVm);
        void AddSelected(ITagItemViewModel tagVm, string text);

        ObservableCollection<ITagItemViewModel> AllTags { get; }
        ObservableCollection<ITagItemViewModel> SelectedTags { get; }
    }

    internal class TagEditorViewModel : ViewModelBase, ITagEditorViewModel
    {
        public TagEditorViewModel(ObservableCollection<ITagItemViewModel> allTags)
        {
            AllTags = allTags;
        }

        public void AddSelected(ITagItemViewModel tagVm)
        {
            AddSelected(tagVm, tagVm.Tag);
        }

        public void AddSelected(ITagItemViewModel tagVm, string text)
        {
            if (tagVm == null)
            {
                if (string.IsNullOrEmpty(text))
                    return;
                tagVm = new TagItemViewModel(text.Trim(), AllTags.Count);
                AllTags.Add(tagVm);
            }

            var selectedVm = new TagItemViewModel(tagVm);
            selectedVm.Delete = new ActionCommand(_ => SelectedTags.Remove(selectedVm));
            SelectedTags.Add(selectedVm);
        }

        public ObservableCollection<ITagItemViewModel> AllTags { get; }
        public ObservableCollection<ITagItemViewModel> SelectedTags { get; } = new();
    }
}
