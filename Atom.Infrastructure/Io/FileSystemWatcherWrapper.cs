using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileSystemWatcher : IDisposable
{
    void IncreaseBuffer();
    bool StartListening(string path, string filter = "*.*");
    void StopListening();

    IObservable<FileSystemEventArgs> Created { get; }
    IObservable<FileSystemEventArgs> Changed { get; }
    IObservable<RenamedEventArgs> Renamed { get; }
    IObservable<FileSystemEventArgs> Deleted { get; }
    IObservable<ErrorEventArgs> Error { get; }
}

public sealed class FileSystemWatcherWrapper : IFileSystemWatcher
{
    private readonly FileSystemWatcher _watcher;
    private readonly ILogger<FileSystemWatcherWrapper> _logger;

    public FileSystemWatcherWrapper(ILogger<FileSystemWatcherWrapper> logger)
    {
        _logger = logger.NotNull();

        _watcher = new FileSystemWatcher
        {
            EnableRaisingEvents = false,
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
        };

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
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    public void IncreaseBuffer()
    {
        _watcher.InternalBufferSize = 65536;
    }

    public bool StartListening(string path, string filter = "*.*")
    {
        _watcher.Path = path;
        _watcher.Filter = filter;

        try
        {
            _watcher.EnableRaisingEvents = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return false;
        }
    }

    public void StopListening()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public IObservable<FileSystemEventArgs> Created { get; }
    public IObservable<FileSystemEventArgs> Changed { get; }
    public IObservable<RenamedEventArgs> Renamed { get; }
    public IObservable<FileSystemEventArgs> Deleted { get; }
    public IObservable<ErrorEventArgs> Error { get; }
}
