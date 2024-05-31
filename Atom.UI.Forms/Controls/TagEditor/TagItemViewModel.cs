using System.Windows.Media;

namespace Genius.Atom.UI.Forms.Controls.TagEditor;

public interface ITagItemViewModel : IComparable
{
    string Tag { get; set; }
    Brush Color { get; set; }
    Brush AltColor { get; set; }

    IActionCommand? Delete { get; set; }
}

public sealed class TagItemViewModel : ViewModelBase, ITagItemViewModel
{
    private readonly Color[] _refColors = new [] {
        System.Windows.Media.Color.FromRgb(82, 107, 64),
        System.Windows.Media.Color.FromRgb(183, 158, 43),
        System.Windows.Media.Color.FromRgb(99, 89, 66),
        System.Windows.Media.Color.FromRgb(177, 119, 37),
        System.Windows.Media.Color.FromRgb(56, 82, 75),
        System.Windows.Media.Color.FromRgb(82, 66, 65),
        System.Windows.Media.Color.FromRgb(132, 89, 50),
        System.Windows.Media.Color.FromRgb(119, 129, 64),
        System.Windows.Media.Color.FromRgb(105, 48, 57),
        System.Windows.Media.Color.FromRgb(165, 89, 34)
    };

    public TagItemViewModel(string tag, int index)
    {
        Tag = tag;

        var color = _refColors[index % _refColors.Length];
        Color = new SolidColorBrush(color);
        AltColor = new SolidColorBrush(ColorHelpers.ChangeColorBrightness(color, -0.5f));
    }

    public TagItemViewModel(ITagItemViewModel reference)
    {
        Guard.NotNull(reference);

        Tag = reference.Tag;
        Color = reference.Color;
        AltColor = reference.AltColor;
    }

    public int CompareTo(object? obj)
    {
        return new TagItemComparer().Compare(this, obj);
    }

    public string Tag
    {
        get => GetOrDefault(string.Empty);
        set => RaiseAndSetIfChanged(value);
    }

    public Brush Color
    {
        get => GetOrDefault(Brushes.Gray);
        set => RaiseAndSetIfChanged(value);
    }

    public Brush AltColor
    {
        get => GetOrDefault(Brushes.Gray);
        set => RaiseAndSetIfChanged(value);
    }

    public IActionCommand? Delete { get; set; }

    public override string ToString()
        => Tag;
}
