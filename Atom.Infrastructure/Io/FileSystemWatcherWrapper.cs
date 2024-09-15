using System.Reactive.Linq;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileSystemWatcher : IDisposable
{
    IObservable<FileSystemEventArgs> Created { get; }
    IObservable<FileSystemEventArgs> Changed { get; }
    IObservable<RenamedEventArgs> Renamed { get; }
    IObservable<FileSystemEventArgs> Deleted { get; }
    IObservable<ErrorEventArgs> Error { get; }
}

public sealed class FileSystemWatcherWrapper : IFileSystemWatcher
{
    private readonly FileSystemWatcher _watcher;

    public FileSystemWatcherWrapper(string path, string filter = "*.*", bool increaseBuffer = false)
    {
        _watcher = new FileSystemWatcher
        {
            EnableRaisingEvents = false,
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };

        if (increaseBuffer)
        {
            _watcher.InternalBufferSize = 65536;
        }

        Created = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Created += h, h => _watcher.Created -= h)
            .Select(x => x.EventArgs);
        Changed = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Changed += h, h => _watcher.Changed -= h)
            .Select(x => x.EventArgs);
        Renamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
            h => _watcher.Renamed += h, h => _watcher.Renamed -= h)
            .Select(x => x.EventArgs);
        Deleted = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
            h => _watcher.Deleted += h, h => _watcher.Deleted -= h)
            .Select(x => x.EventArgs);
        Error = Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
            h => _watcher.Error += h, h => _watcher.Error -= h)
            .Select(x => x.EventArgs);

        _watcher.Path = path;
        _watcher.Filter = filter;
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    public IObservable<FileSystemEventArgs> Created { get; }
    public IObservable<FileSystemEventArgs> Changed { get; }
    public IObservable<RenamedEventArgs> Renamed { get; }
    public IObservable<FileSystemEventArgs> Deleted { get; }
    public IObservable<ErrorEventArgs> Error { get; }
}
