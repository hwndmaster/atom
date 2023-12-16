using System.Windows.Input;

namespace Genius.Atom.UI.Forms;

public sealed class DropAreaViewModel : ViewModelBase
{
    public DropAreaViewModel(string caption, ICommand dropAction)
    {
        Caption = caption.NotNull();
        DropAction = dropAction.NotNull();
    }

    public DropAreaViewModel(string caption, Action<object> dropAction)
    {
        Caption = caption.NotNull();
        DropAction = new ActionCommand(obj => dropAction(obj.NotNull()));
    }

    public string Caption { get; }
    public ICommand DropAction { get; }
}
