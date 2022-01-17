using System.ComponentModel;
using System.Windows.Media.Imaging;
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
                LogLevel.Warning => "Warning16",
                LogLevel.Error => "Error16",
                LogLevel.Critical => "Alert32",
                {} => null
            };
            if (icon is null)
                return null;
            return (BitmapImage)Application.Current.FindResource(icon);
        }
    }

    [Browsable(false)]
    public bool IsSeverityCritical => Severity == LogLevel.Critical;

    [Icon("Copy16")]
    public IActionCommand CopyToClipboardCommand { get; }
}
