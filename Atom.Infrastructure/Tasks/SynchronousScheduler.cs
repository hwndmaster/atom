using Genius.Atom.Infrastructure.Threading;

namespace Genius.Atom.Infrastructure.Tasks;

/// <summary>
///   A service to schedule actions synchronously.
/// </summary>
public interface ISynchronousScheduler
{
    void Schedule(Action action);
    void Schedule(Func<Task> asyncAction);
}

internal sealed class SynchronousScheduler : IDisposable, ISynchronousScheduler
{
    private readonly Queue<Action> _actions = new();
    private readonly JoinableTaskHelper _joinableTask = new();
    private readonly InternalSynchronizationContext _synchronizationContext;
    private bool _running;
    private int _asyncRunning;
    private bool _disposed;

    public SynchronousScheduler()
    {
        _synchronizationContext = new(this);
    }

    public void Schedule(Action action)
    {
        Guard.NotNull(action);

        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SynchronousScheduler));
        }

        lock (_actions)
        {
            if ( _actions.Count == 0 && !_running && _asyncRunning == 0)
            {
                _running = true;
            }
            else
            {
                _actions.Enqueue(action);
                return;
            }
        }

        Run(action);
    }

    public void Schedule(Func<Task> asyncAction)
    {
        Guard.NotNull(asyncAction);

        Schedule(() =>
        {
            try
            {
                Interlocked.Increment(ref _asyncRunning);

                _joinableTask.Factory.Run(async () =>
                    await asyncAction().ConfigureAwait(true)
                );
            }
            finally
            {
                Interlocked.Decrement(ref _asyncRunning);
            }
        });
    }

    internal void ScheduleFromSynchronizationContext(Action action)
    {
        if (_disposed)
        {
            return;
        }

        Run(action);
    }

    private Action? GetNextAction()
    {
        if (_disposed)
        {
            return null;
        }

        lock (_actions)
        {
            if (_asyncRunning > 0 || _actions.Count == 0)
            {
                _running = false;
                return null;
            }

            return _actions.Dequeue();
        }
    }

    private void Run(Action? action)
    {
        var previousSynchronizationContext = SynchronizationContext.Current;

        try
        {
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            while (action is not null)
            {
                action();

                action = GetNextAction();
            }
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousSynchronizationContext);
        }
    }

    public void Dispose()
    {
        _disposed = true;

        lock (_actions)
        {
            _actions.Clear();
        }

        _joinableTask.Dispose();
    }


    private sealed class InternalSynchronizationContext : SynchronizationContext
    {
        private readonly SynchronousScheduler _scheduler;

        public InternalSynchronizationContext(SynchronousScheduler scheduler)
        {
            _scheduler = scheduler.NotNull();
        }

        public override SynchronizationContext CreateCopy()
        {
            return new InternalSynchronizationContext(_scheduler);
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            _scheduler.ScheduleFromSynchronizationContext(() => d(state));
        }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException();
        }
    }
}
