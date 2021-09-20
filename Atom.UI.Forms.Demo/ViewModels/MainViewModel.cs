using System;
using System.Collections.ObjectModel;
using Genius.Atom.UI.Forms.Controls.TagEditor;

namespace Genius.Atom.UI.Forms.Demo.ViewModels
{
    public class MainViewModel
    {
        private readonly IViewModelFactory _vmFactory;

        public MainViewModel(IViewModelFactory vmFactory)
        {
            _vmFactory = vmFactory;
        }

        public MainViewModel()
        {
            var tagsObservables = new ObservableCollection<ITagItemViewModel>();
            string[] tags = new [] { "adipiscing", "aliquam", "amet", "consectetur", "consequat", "cursus", "dignissim", "dolor", "dui", "eget", "elit", "enim", "est", "et", "euismod", "fusce", "ipsum", "lectus", "ligula", "lobortis", "lorem", "mauris", "maximus", "nisi", "odio", "pellentesque", "potenti", "quis", "rutrum", "sed", "sem", "sit", "suscipit", "suspendisse", "tempus", "tincidunt", "tortor", "tristique", "turpis", "vel", "venenatis", "vivamus", "volutpat" };
            for (var i = 0; i < tags.Length; i++)
            {
                tagsObservables.Add(_vmFactory.CreateTagItemViewModel(tags[i], i));
            }
            TagsForControl1 = _vmFactory.CreateTagEditorViewModel(tagsObservables);

            TagsForControl1.AddSelected(tagsObservables[0]);
            TagsForControl1.AddSelected(tagsObservables[1]);
            TagsForControl1.AddSelected(tagsObservables[2]);

            GridItems.Add(new SampleData { Tags = _vmFactory.CreateTagEditorViewModel(tagsObservables) });
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[0]);
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[4]);
            GridItems[0].Tags.AddSelected(GridItems[0].Tags.AllTags[8]);
            GridItems.Add(new SampleData { Tags = _vmFactory.CreateTagEditorViewModel(tagsObservables) });
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[2]);
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[6]);
            GridItems[1].Tags.AddSelected(GridItems[0].Tags.AllTags[9]);
        }

        public ITagEditorViewModel TagsForControl1 { get; }

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

        public ITagEditorViewModel Tags
        {
            get => GetOrDefault<ITagEditorViewModel>();
            set => RaiseAndSetIfChanged(value);
        }
    }
}