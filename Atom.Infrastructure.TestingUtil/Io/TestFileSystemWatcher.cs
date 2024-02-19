using System.Reactive.Subjects;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public sealed class TestFileSystemWatcherFactory : IFileSystemWatcherFactory
{
    public TestFileSystemWatcher? InstanceToReturn { get; set; }
    public TestFileSystemWatcher? RecentlyCreatedInstance { get; private set; }
    public int InstancesCreated { get; private set; } = 0;

    public IFileSystemWatcher? Create(string path, string filter = "*.*", bool increaseBuffer = false)
    {
        var instance = InstanceToReturn ?? new TestFileSystemWatcher(path, filter, increaseBuffer);
        RecentlyCreatedInstance = instance;
        InstancesCreated++;
        return instance;
    }
}

public sealed class TestFileSystemWatcher : IFileSystemWatcher
{
    private readonly Subject<FileSystemEventArgs> _created = new();
    private readonly Subject<FileSystemEventArgs> _changed = new();
    private readonly Subject<RenamedEventArgs> _renamed = new();
    private readonly Subject<FileSystemEventArgs> _deleted = new();
    private readonly Subject<ErrorEventArgs> _error = new();

    public TestFileSystemWatcher(string path, string filter, bool increaseBuffer)
    {
        ListeningPath = path;
        ListeningFilter = filter;
        IsBufferIncreased = increaseBuffer;
        IsListening = true;
    }

    public IObservable<FileSystemEventArgs> Created => _created;
    public IObservable<FileSystemEventArgs> Changed => _changed;
    public IObservable<RenamedEventArgs> Renamed => _renamed;
    public IObservable<FileSystemEventArgs> Deleted => _deleted;
    public IObservable<ErrorEventArgs> Error => _error;

    public bool IsBufferIncreased { get; private set; }
    public bool IsListening { get; private set; }
    public string? ListeningPath { get; private set; }
    public string? ListeningFilter { get; private set; }

    public void IncreaseBuffer()
    {
        IsBufferIncreased = true;
    }

    public void OnCreated(string directory, string? name)
    {
        _created.OnNext(new FileSystemEventArgs(WatcherChangeTypes.Created, directory, name));
    }

    public void OnChanged(string directory, string? name)
    {
        _changed.OnNext(new FileSystemEventArgs(WatcherChangeTypes.Changed, directory, name));
    }

    public void OnRenamed(string directory, string? name, string? oldName)
    {
        _renamed.OnNext(new RenamedEventArgs(WatcherChangeTypes.Renamed, directory, name, oldName));
    }

    public void OnDeleted(string directory, string? name)
    {
        _deleted.OnNext(new FileSystemEventArgs(WatcherChangeTypes.Deleted, directory, name));
    }

    public void OnError(Exception exception)
    {
        _error.OnNext(new ErrorEventArgs(exception));
    }

    public bool StartListening(string path, string filter = "*.*")
    {
        IsListening = true;
        ListeningPath = path;
        ListeningFilter = filter;
        return true;
    }

    public void StopListening()
    {
        IsListening = false;
    }

    public void Dispose()
    {
        StopListening();
    }
}
