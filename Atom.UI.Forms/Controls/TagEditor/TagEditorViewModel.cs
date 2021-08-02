using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Genius.Atom.UI.Forms.ViewModels;

namespace Genius.Atom.UI.Forms.Controls.TagEditor
{
    public class TagEditorViewModel : ViewModelBase
    {
        public TagEditorViewModel(ObservableCollection<TagItemViewModel> allTags)
        {
            AllTags = allTags;
        }

        public static ObservableCollection<TagItemViewModel> CreateTagItemViewModels(IEnumerable<string> tags)
        {
            var items = tags.Select((tag, i) => new TagItemViewModel(tag, i));
            return new ObservableCollection<TagItemViewModel>(items);
        }

        public void AddSelected(TagItemViewModel tagVm, string text)
        {
            if (tagVm == null)
            {
                if (string.IsNullOrEmpty(text))
                    return;
                tagVm = new TagItemViewModel(text.Trim(), AllTags.Count);
                AllTags.Add(tagVm);
            }
            SelectedTags.Add(new TagItemViewModel(tagVm) {
                Delete = new ActionCommand(_ => {
                    SelectedTags.Remove(tagVm);
                })
            });
        }

        public ObservableCollection<TagItemViewModel> AllTags { get; }
        public ObservableCollection<TagItemViewModel> SelectedTags { get; } = new();
    }
}
