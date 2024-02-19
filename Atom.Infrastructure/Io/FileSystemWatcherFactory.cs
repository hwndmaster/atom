using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileSystemWatcherFactory
{
    IFileSystemWatcher? Create(string path, string filter = "*.*", bool increaseBuffer = false);
}

internal sealed class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
    private readonly ILogger<FileSystemWatcherWrapper> _logger;

    public FileSystemWatcherFactory(ILogger<FileSystemWatcherWrapper> logger)
    {
        _logger = logger.NotNull();
    }

    public IFileSystemWatcher? Create(string path, string filter = "*.*", bool increaseBuffer = false)
    {
        try
        {
            return new FileSystemWatcherWrapper(_logger, path, filter, increaseBuffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }
}
