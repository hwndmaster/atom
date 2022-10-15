namespace Genius.Atom.UI.Forms.Demo.ViewModels;

internal class SampleDataFactory : IFactory<SampleData>
{
    private readonly IAtomViewModelFactory _vmFactory;
    private readonly TagsContext _tagsContext;

    public SampleDataFactory(IAtomViewModelFactory vmFactory, TagsContext tagsContext)
    {
        _vmFactory = vmFactory;
        _tagsContext = tagsContext;
    }

    public SampleData Create()
    {
        return new SampleData
        {
            Tags = _vmFactory.CreateTagEditorViewModel(_tagsContext.TagsObservables)
        };
    }
}
