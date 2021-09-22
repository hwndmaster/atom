using System.Collections.ObjectModel;
using System.Linq;

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
            text = text.Trim();

            if (tagVm == null)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                if (!AllTags.Any(x => x.Tag == text))
                {
                    tagVm = new TagItemViewModel(text.Trim(), AllTags.Count);
                    AllTags.Add(tagVm);
                }
            }

            if (SelectedTags.Any(x => x.Tag == text))
            {
                return;
            }

            var selectedVm = new TagItemViewModel(tagVm);
            selectedVm.Delete = new ActionCommand(_ => SelectedTags.Remove(selectedVm));
            SelectedTags.Add(selectedVm);
        }

        public ObservableCollection<ITagItemViewModel> AllTags { get; }
        public ObservableCollection<ITagItemViewModel> SelectedTags { get; } = new();
    }
}
