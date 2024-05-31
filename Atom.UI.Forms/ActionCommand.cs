using System.Reactive.Subjects;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.UI.Forms;

public interface IActionCommand : ICommand
{
    IObservable<bool> Executed { get; }
}

public interface IActionCommand<in T> : IActionCommand
{
    bool CanExecute(T parameter);
    Task ExecuteAsync(T parameter);
}

public sealed class ActionCommand : ActionCommand<object?>
{
    public ActionCommand()
        : base(_ => { }, null)
    {
    }

    public ActionCommand(Func<object?, Task> asyncAction)
        : base(asyncAction, null)
    {
    }

    public ActionCommand(Action<object?> action)
        : base(action, null)
    {
    }

    public ActionCommand(Action<object?> action, Predicate<object?>? canExecute)
        : base(action, canExecute)
    {
    }

    public ActionCommand(Func<object?, Task> asyncAction, Predicate<object?>? canExecute)
        : base(asyncAction, canExecute)
    {
    }
}

public class ActionCommand<T> : IActionCommand<T>
{
    private readonly Func<T, Task> _asyncAction;
    private readonly Predicate<T>? _canExecute;
    private readonly Subject<bool> _executed = new();

    public ActionCommand()
        : this (_ => { }, null)
    {
    }

    public ActionCommand(Func<T, Task> asyncAction)
        : this (asyncAction, null)
    {
    }

    public ActionCommand(Action<T> action)
        : this (action, null)
    {
    }

    public ActionCommand(Action<T> action, Predicate<T>? canExecute)
        : this ((o) => { action(o); return Task.CompletedTask; }, canExecute)
    {
    }

    public ActionCommand(Func<T, Task> asyncAction, Predicate<T>? canExecute)
    {
        _asyncAction = asyncAction.NotNull();
        _canExecute = canExecute;
    }

    public bool CanExecute(T parameter) => CanExecuteInternal(parameter);
    public Task ExecuteAsync(T parameter) => ExecuteInternalAsync(parameter);

    public bool CanExecute(object? parameter)
    {
        if (parameter is T value)
            return CanExecuteInternal(value);

        return CanExecuteInternal(default!);
    }

    public void Execute(object? parameter)
    {
        Task task;

        if (parameter is T value)
            task = ExecuteInternalAsync(value);
        else
            task = ExecuteInternalAsync(default!);

#pragma warning disable VSTHRD110 // Observe result of async calls
        task.ContinueWith(t =>
        {
            var logger = Module.ServiceProvider.GetRequiredService<ILogger<ActionCommand>>();
            logger.LogError(t.Exception, "ActionCommand execution failure. Parameter = '{Parameter}'", parameter);
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
#pragma warning restore VSTHRD110 // Observe result of async calls
    }

    private bool CanExecuteInternal(T parameter)
    {
        if (_canExecute is not null)
        {
            return _canExecute.Invoke(parameter);
        }

        return true;
    }

    private async Task ExecuteInternalAsync(T parameter)
    {
        try
        {
            var task = _asyncAction.Invoke(parameter);
            await task;
            if (task is Task<bool> taskBool)
            {
#pragma warning disable VSTHRD103 // Call async methods when in an async method
#pragma warning disable CA1849 // Call async methods when in an async method
                // Already awaited two lines above
                _executed.OnNext(taskBool.Result);
#pragma warning restore CA1849 // Call async methods when in an async method
#pragma warning restore VSTHRD103 // Call async methods when in an async method
            }
            else
            {
                _executed.OnNext(true);
            }
        }
        catch (Exception ex)
        {
            var message = $"Action command execution failure. Parameter = '{parameter}'";
            var logger = Module.ServiceProvider.GetRequiredService<ILogger<ActionCommand>>();
            logger.LogError(ex, message);
            MessageBox.Show(message + ". Check logs for details.", "Action failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public IObservable<bool> Executed => _executed;
}
