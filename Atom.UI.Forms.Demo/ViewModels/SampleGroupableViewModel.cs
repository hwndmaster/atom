using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Demo.ViewModels;

public class SampleGroupableViewModel : ViewModelBase, IGroupableViewModel
{
    public SampleGroupableViewModel()
    {
        RunItCommand = new ActionCommand(_ =>
        {
            App.ServiceProvider.GetRequiredService<IUserInteraction>().ShowInformation($"Run group '{GroupTitle}'");
        });
    }

    public string GroupTitle
    {
        get => GetOrDefault<string>();
        set => RaiseAndSetIfChanged(value);
    }

    public int? ItemCount => null; // Will be determined automatically

    public bool IsExpanded
    {
        get => GetOrDefault<bool>();
        set => RaiseAndSetIfChanged(value);
    }

    public IEnumerable<IGroupingField> ExtraGroupFields
    {
        get
        {
            yield return new ValueGroupingField("Abbr", GroupTitle[0], null, "This is an abbreviation");
            yield return new ValueGroupingField("Random Number", Guid.NewGuid().GetHashCode(), new PropertyValueStringConverter("#,###,##0"));
            yield return new CommandGroupingField("RUN IT!", RunItCommand, null, "Do something");
        }
    }

    public IActionCommand RunItCommand { get; }
}
