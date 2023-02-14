using Genius.Atom.Infrastructure.Tasks;

namespace Genius.Atom.Infrastructure.TestingUtil.Tasks;

public sealed class TestSynchronousScheduler : ISynchronousScheduler
{
    private readonly SynchronousScheduler _origin = new();

    public void Schedule(Action action)
    {
        _origin.Schedule(action);
    }

    public void ScheduleAsync(Func<Task> asyncAction)
    {
        _origin.ScheduleAsync(asyncAction);
    }
}
