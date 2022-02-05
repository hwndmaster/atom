namespace Genius.Atom.UI.Forms.Demo.ViewModels;

public interface ISampleDataFactory
{
    SampleData Create();
}

internal class SampleDataFactory : ISampleDataFactory
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
