using System.Collections.ObjectModel;
using System.ComponentModel;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Demo.ViewModels;

public class SampleData : ViewModelBase, IEditable, ISelectable
{
    public SampleData()
    {
        Selection.Add("Test");
        Selection.Add("Foo");
        Selection.Add("Bar");
        Selection.Add("Doe");
    }

    public SampleGroupableViewModel Groupable
    {
        get => GetOrDefault<SampleGroupableViewModel>();
        set => RaiseAndSetIfChanged(value);
    }

    public string Name
    {
        get => GetOrDefault<string>();
        set => RaiseAndSetIfChanged(value);
    }

    public int Number
    {
        get => GetOrDefault<int>();
        set => RaiseAndSetIfChanged(value);
    }

    public DateTime DateTime
    {
        get => GetOrDefault<DateTime>();
        set => RaiseAndSetIfChanged(value);
    }

    public bool Flag
    {
        get => GetOrDefault<bool>();
        set => RaiseAndSetIfChanged(value);
    }

    [Browsable(false)]
    public ObservableCollection<string> Selection { get; } = new ();

    [SelectFromList(nameof(Selection))]
    public string Selected
    {
        get => GetOrDefault<string>();
        set => RaiseAndSetIfChanged(value);
    }

    public ITagEditorViewModel Tags
    {
        get => GetOrDefault<ITagEditorViewModel>();
        set => RaiseAndSetIfChanged(value);
    }

    public bool IsEditing
    {
        get => GetOrDefault(false);
        set => RaiseAndSetIfChanged(value);
    }

    public bool IsSelected
    {
        get => GetOrDefault(false);
        set => RaiseAndSetIfChanged(value);
    }
}
