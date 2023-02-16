using System.Diagnostics;

namespace Genius.Atom.UI.Forms.Demo.ViewModels;

internal class SampleDataFactory : IFactory<SampleData>
{
    private readonly IAtomViewModelFactory _vmFactory;
    private readonly TagsContext _tagsContext;
    private SampleGroupableViewModel? _groupable;
    private SampleGroupableViewModel? _dummyGroup;

    public SampleDataFactory(IAtomViewModelFactory vmFactory, TagsContext tagsContext)
    {
        _vmFactory = vmFactory;
        _tagsContext = tagsContext;
    }

    public SampleDataFactory ForGroup(SampleGroupableViewModel? groupable)
    {
        _groupable = groupable;
        return this;
    }

    public SampleData Create()
    {
        var groupable = _groupable ?? (_dummyGroup ??= new SampleGroupableViewModel
        {
            GroupTitle = "Dummy group",
            IsExpanded = true
        });

        return new SampleData
        {
            Groupable = groupable,
            Tags = _vmFactory.CreateTagEditorViewModel(_tagsContext.TagsObservables)
        };
    }
}
