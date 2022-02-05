using System.Collections.ObjectModel;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Demo;

public class TagsContext
{
    public TagsContext(IAtomViewModelFactory vmFactory)
    {
        TagsObservables = new();

        string[] tags = new [] { "adipiscing", "aliquam", "amet", "consectetur", "consequat", "cursus", "dignissim", "dolor", "dui", "eget", "elit", "enim", "est", "et", "euismod", "fusce", "ipsum", "lectus", "ligula", "lobortis", "lorem", "mauris", "maximus", "nisi", "odio", "pellentesque", "potenti", "quis", "rutrum", "sed", "sem", "sit", "suscipit", "suspendisse", "tempus", "tincidunt", "tortor", "tristique", "turpis", "vel", "venenatis", "vivamus", "volutpat" };
        for (var i = 0; i < tags.Length; i++)
        {
            TagsObservables.Add(vmFactory.CreateTagItemViewModel(tags[i], i));
        }
    }

    public ObservableCollection<ITagItemViewModel> TagsObservables { get; }
}
