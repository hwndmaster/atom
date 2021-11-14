using System.Windows.Media;

namespace Genius.Atom.UI.Forms;

public interface ITitledItemViewModel : IViewModel
{
    Guid Id { get; }
    string Name { get; }
}

public interface ITitledItemWithImageViewModel : ITitledItemViewModel
{
    ImageSource Image { get; }
}
