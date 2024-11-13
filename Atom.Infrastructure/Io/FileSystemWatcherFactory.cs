using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Io;

/// <summary>
///   Represents a factory for creating instances of <see cref="IFileSystemWatcher"/>.
/// </summary>
public interface IFileSystemWatcherFactory
{
    /// <summary>
    ///   Creates a new instance of <see cref="IFileSystemWatcher"/>.
    ///   If the creation fails, logs the exception and returns null.
    /// </summary>
    /// <param name="path">The path to the directory to watch.</param>
    /// <param name="filter">The filter string used to determine what files are monitored in a directory.</param>
    /// <param name="increaseBuffer">Whether to increase the internal buffer size of the watcher.</param>
    IFileSystemWatcher? Create(string path, string filter = "*.*", bool increaseBuffer = false);
}

internal sealed class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
    private readonly ILogger<FileSystemWatcherFactory> _logger;

    public FileSystemWatcherFactory(ILogger<FileSystemWatcherFactory> logger)
    {
        _logger = logger.NotNull();
    }

    public IFileSystemWatcher? Create(string path, string filter = "*.*", bool increaseBuffer = false)
    {
        try
        {
            return new FileSystemWatcherWrapper(path, filter, increaseBuffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}
