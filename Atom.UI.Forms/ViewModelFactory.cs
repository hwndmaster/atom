using System.Collections.ObjectModel;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms;

public interface IAtomViewModelFactory
{
    ITagEditorViewModel CreateTagEditorViewModel(ObservableCollection<ITagItemViewModel> allTags);
    ITagItemViewModel CreateTagItemViewModel(string tag, int index);
    ITagItemViewModel CreateTagItemViewModel(ITagItemViewModel reference);
    ObservableCollection<ITagItemViewModel> CreateTagItemViewModels(IEnumerable<string> tags);
}

internal sealed class ViewModelFactory : IAtomViewModelFactory
{
    public ITagEditorViewModel CreateTagEditorViewModel(ObservableCollection<ITagItemViewModel> allTags)
    {
        return new TagEditorViewModel(allTags);
    }

    public ITagItemViewModel CreateTagItemViewModel(string tag, int index)
    {
        return new TagItemViewModel(tag, index);
    }

    public ITagItemViewModel CreateTagItemViewModel(ITagItemViewModel reference)
    {
        return new TagItemViewModel(reference);
    }

    public ObservableCollection<ITagItemViewModel> CreateTagItemViewModels(IEnumerable<string> tags)
    {
        var items = tags.Select((tag, i) => new TagItemViewModel(tag, i));
        return new ObservableCollection<ITagItemViewModel>(items);
    }
}
