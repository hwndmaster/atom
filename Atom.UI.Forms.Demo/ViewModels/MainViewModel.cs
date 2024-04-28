using System.Collections.ObjectModel;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.Controls.TagEditor;
using Genius.Atom.UI.Forms.Demo.AutoGridBuilders;

namespace Genius.Atom.UI.Forms.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    public MainViewModel(IAtomViewModelFactory vmFactory, TagsContext tagsContext,
        IFactory<SampleData> sampleDataFactory, SampleDataAutoGridBuilder autoGridBuilder)
    {
        Guard.NotNull(vmFactory);

        AutoGridBuilder = autoGridBuilder.NotNull();
        var sampleDataFactoryImpl = (SampleDataFactory)sampleDataFactory;

        TagsForControl1 = vmFactory.CreateTagEditorViewModel(tagsContext.TagsObservables);

        TagsForControl1.AddSelected(tagsContext.TagsObservables[0]);
        TagsForControl1.AddSelected(tagsContext.TagsObservables[1]);
        TagsForControl1.AddSelected(tagsContext.TagsObservables[2]);

        var group = new SampleGroupableViewModel() { GroupTitle = "Group 1", IsExpanded = true };

        GridItems.Add(sampleDataFactoryImpl.ForGroup(group).Create());
        GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[0]);
        GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[4]);
        GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[8]);

        var item2 = sampleDataFactoryImpl.ForGroup(group).Create();
        item2.Name = "Very very very very very very very very very very very very very long name";
        item2.DateTime = DateTime.Now.AddDays(100);
        GridItems.Add(item2);
        GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[2]);
        GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[6]);
        GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[9]);

        AddRowCommand = new ActionCommand(_ => {
            var addedItem = sampleDataFactory.Create();
            GridItems.Add(addedItem);
            foreach (var item in GridItems)
                item.IsSelected = false;
            addedItem.IsSelected = true;
            addedItem.IsEditing = true;
        });
    }

    public IAutoGridBuilder AutoGridBuilder { get; }

    public ITagEditorViewModel TagsForControl1 { get; }

    public ObservableCollection<SampleData> GridItems { get; } = new();

    public IActionCommand AddRowCommand { get; }
}
