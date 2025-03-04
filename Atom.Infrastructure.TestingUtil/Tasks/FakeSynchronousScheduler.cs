using Genius.Atom.Infrastructure.Tasks;

namespace Genius.Atom.Infrastructure.TestingUtil.Tasks;

public sealed class FakeSynchronousScheduler : ISynchronousScheduler, IDisposable
{
    private readonly SynchronousScheduler _origin = new();

    public void Dispose() => _origin.Dispose();

    public void Schedule(Action action)
    {
        _origin.Schedule(action);
    }

    public void Schedule(Func<Task> asyncAction)
    {
        _origin.Schedule(asyncAction);
    }
}
