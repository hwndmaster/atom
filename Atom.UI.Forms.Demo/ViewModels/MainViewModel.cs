using System;
using System.Collections.ObjectModel;
using Genius.Atom.UI.Forms.Controls.TagEditor;
using Genius.Atom.UI.Forms.ViewModels;

namespace Genius.Atom.UI.Forms.Demo.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            var tagsObservables = new ObservableCollection<TagItemViewModel>();
            string[] tags = new [] { "adipiscing", "aliquam", "amet", "consectetur", "consequat", "cursus", "dignissim", "dolor", "dui", "eget", "elit", "enim", "est", "et", "euismod", "fusce", "ipsum", "lectus", "ligula", "lobortis", "lorem", "mauris", "maximus", "nisi", "odio", "pellentesque", "potenti", "quis", "rutrum", "sed", "sem", "sit", "suscipit", "suspendisse", "tempus", "tincidunt", "tortor", "tristique", "turpis", "vel", "venenatis", "vivamus", "volutpat" };
            for (var i = 0; i < tags.Length; i++)
            {
                tagsObservables.Add(new TagItemViewModel(tags[i], i));
            }
            TagsForControl1 = new TagEditorViewModel(tagsObservables);

            TagsForControl1.AddSelected(tagsObservables[0]);
            TagsForControl1.AddSelected(tagsObservables[1]);
            TagsForControl1.AddSelected(tagsObservables[2]);

            GridItems.Add(new SampleData { Tags = new TagEditorViewModel(tagsObservables) });
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[0]);
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[4]);
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[8]);
            GridItems.Add(new SampleData { Tags = new TagEditorViewModel(tagsObservables) });
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[2]);
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[6]);
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[9]);
        }

        public TagEditorViewModel TagsForControl1 { get; }

        public ObservableCollection<SampleData> GridItems { get; } = new();
    }

    public class SampleData : ViewModelBase
    {
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

        public TagEditorViewModel Tags
        {
            get => GetOrDefault<TagEditorViewModel>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}