using System.ComponentModel;

namespace Genius.Atom.UI.Forms;

public interface ITabViewModel : IViewModel
{
    IActionCommand Activated { get; }
    IActionCommand Deactivated { get; }
}

public abstract class TabViewModelBase : ViewModelBase, ITabViewModel
{
    [Browsable(false)]
    public IActionCommand Activated { get; } = new ActionCommand();
    [Browsable(false)]
    public IActionCommand Deactivated { get; } = new ActionCommand();
}
