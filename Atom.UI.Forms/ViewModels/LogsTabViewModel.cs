using System.Reactive.Disposables;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.UI.Forms.ViewModels;

public interface ILogsTabViewModel : ITabViewModel
{
}

internal sealed class LogsTabViewModel : TabViewModelBase, ILogsTabViewModel, IDisposable
{
    private readonly Disposer _disposer = new();

    public LogsTabViewModel(IEventBus eventBus, IUiDispatcher uiDispatcher)
    {
        eventBus.WhenFired<LogEvent>()
            .Subscribe(x => {
                uiDispatcher.Invoke(() =>
                    LogItems.Add(new LogItemViewModel { Severity = x.Severity, Logger = x.Logger, Message = x.Message }));
            })
            .DisposeWith(_disposer);

        CleanLogCommand = new ActionCommand(_ => LogItems.Clear());

        LogItems.WhenCollectionChanged()
            .Subscribe(args =>
            {
                if (HasNewErrors)
                    return;
                HasNewErrors = args.NewItems?.Cast<ILogItemViewModel>()
                    .Any(x => x.Severity >= LogLevel.Error) ?? false;
            })
            .DisposeWith(_disposer);

        Activated.Executed.Subscribe(_ => HasNewErrors = false).DisposeWith(_disposer);
        Deactivated.Executed.Subscribe(_ => HasNewErrors = false).DisposeWith(_disposer);
    }

    public DelayedObservableCollection<ILogItemViewModel> LogItems { get; }
        = new TypedObservableCollection<ILogItemViewModel, LogItemViewModel>();

    public bool HasNewErrors
    {
        get => GetOrDefault(false);
        set => RaiseAndSetIfChanged(value);
    }

    public IActionCommand CleanLogCommand { get; }

    public void Dispose()
    {
        _disposer.Dispose();
    }
}
