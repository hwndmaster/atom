namespace Genius.Atom.Infrastructure;

public interface ISynchronousScheduler
{
    void Schedule(Action action);
    void ScheduleAsync(Func<Task> asyncAction);
}

internal sealed class SynchronousScheduler : IDisposable, ISynchronousScheduler
{
    private readonly Queue<Action> _actions = new();
    private readonly InternalSynchronizationContext _synchronizationContext;
    private bool _running = false;
    private int _asyncRunning = 0;
    private bool _disposed = false;

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
                Run(action);
            }
            else
            {
                _actions.Enqueue(action);
            }
        }
    }

    public void ScheduleAsync(Func<Task> asyncAction)
    {
        Guard.NotNull(asyncAction);

        Schedule(async () =>
        {
            try
            {
                Interlocked.Increment(ref _asyncRunning);

                await asyncAction().ConfigureAwait(true);
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
