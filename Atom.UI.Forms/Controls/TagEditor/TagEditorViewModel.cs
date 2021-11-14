using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Genius.Atom.UI.Forms.Controls.TagEditor;

public interface ITagEditorViewModel
{
    void AddSelected(ITagItemViewModel tagVm);
    void AddSelected(ITagItemViewModel? tagVm, string text);
    void SetSelected(IEnumerable<string> tags, bool setDirty = true);

    ObservableCollection<ITagItemViewModel> AllTags { get; }
    ObservableCollection<ITagItemViewModel> SelectedTags { get; }
}

internal class TagEditorViewModel : ViewModelBase, ITagEditorViewModel, IHasDirtyFlag
{
    private bool _dirtyPaused = false;

    public TagEditorViewModel(ObservableCollection<ITagItemViewModel> allTags)
    {
        AllTags = allTags;

        SelectedTags.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs args) =>
        {
            if (!_dirtyPaused)
                IsDirty = true;
        };
    }

    public void AddSelected(ITagItemViewModel tagVm)
    {
        AddSelected(tagVm, tagVm.Tag);
    }

    public void AddSelected(ITagItemViewModel? tagVm, string text)
    {
        text = text.Trim();

        if (tagVm is null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            tagVm = AllTags.FirstOrDefault(x => x.Tag == text);
            if (tagVm is null)
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

    public void SetSelected(IEnumerable<string> tags, bool setDirty = true)
    {
        if (tags == null)
            return;

        _dirtyPaused = !setDirty;
        foreach (var tag in tags)
        {
            AddSelected(null, tag);
        }
        _dirtyPaused = false;
    }

    public ObservableCollection<ITagItemViewModel> AllTags { get; }
    public ObservableCollection<ITagItemViewModel> SelectedTags { get; } = new();
    public bool IsDirty { get; set; }
}
