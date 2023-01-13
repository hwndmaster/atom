using System.ComponentModel;
using System.Windows.Media.Imaging;
using Genius.Atom.UI.Forms.Wpf;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.UI.Forms.ViewModels;

public interface ILogItemViewModel : IViewModel
{
    LogLevel Severity { get; }
}

internal sealed class LogItemViewModel : ViewModelBase, ILogItemViewModel
{
    public LogItemViewModel()
    {
        CopyToClipboardCommand = new ActionCommand(_ =>
            Clipboard.SetText(Message));
    }

    [IconSource(nameof(SeverityIcon), 16d)]
    public LogLevel Severity { get; set; }
    public string Logger { get; set; } = string.Empty;
    [Greedy]
    public string Message { get; set; } = string.Empty;

    [Browsable(false)]
    public BitmapImage? SeverityIcon
    {
        get
        {
            var icon = Severity switch
            {
                LogLevel.Warning => ImageStock.Warning16,
                LogLevel.Error => ImageStock.Error16,
                LogLevel.Critical => ImageStock.Alert32,
                {} => null
            };
            if (icon is null)
                return null;
            return (BitmapImage)Application.Current.FindResource(icon);
        }
    }

    [Browsable(false)]
    public bool IsSeverityCritical => Severity == LogLevel.Critical;

    [Icon(ImageStock.Copy16)]
    public IActionCommand CopyToClipboardCommand { get; }
}
