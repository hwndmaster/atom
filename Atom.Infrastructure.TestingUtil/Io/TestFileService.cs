using System.Text;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public record FileContext(string FullName, byte[] Content);

public sealed class TestFileService : IFileService
{
    private readonly Dictionary<string, FileContext> _files = new();
    private readonly HashSet<string> _folders = new();
    private readonly bool _ignoreCase;

    public TestFileService(bool ignoreCase = true)
    {
        _ignoreCase = ignoreCase;
    }

    public void AddFile(string path, byte[]? content = null)
    {
        var key = CreatePathKey(path);
        _files[key] = new FileContext(path, content ?? Array.Empty<byte>());
        _folders.Add(Path.GetDirectoryName(key).NotNull());
    }

    public void RemoveFile(string path)
    {
        var key = CreatePathKey(path);
        _files.Remove(key);
    }

    public void RenameFile(string path, string newPath)
    {
        var oldPathKey = CreatePathKey(path);
        var newPathKey = CreatePathKey(newPath);

        if (!_files.TryGetValue(oldPathKey, out var context))
        {
            throw new KeyNotFoundException($"The file with path '{path}' wasn't added beforehand.");
        }

        _files.Remove(oldPathKey);
        _files.Add(newPathKey, context with { FullName = newPath });
    }

    public void EnsureDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var key = CreatePathKey(path);
        if (!_folders.Contains(key))
        {
            _folders.Add(key);
        }
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
    {
        return _files
            .Where(x => path.Equals(Path.GetDirectoryName(x.Key), _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            .Select(x => x.Value.FullName);
    }

    public bool FileExists(string path)
    {
        var key = CreatePathKey(path);
        return _files.ContainsKey(key);
    }

    public bool IsDirectory(string path)
    {
        return !FileExists(path);
    }

    public Stream OpenRead(string path)
    {
        var key = CreatePathKey(path);
        if (!_files.TryGetValue(key, out var fileContext))
        {
            throw new FileNotFoundException(path);
        }

        return new MemoryStream(fileContext.Content);
    }

    public Stream OpenReadNoLock(string path)
        => OpenRead(path);

    public bool PathExists(string path)
    {
        var key = CreatePathKey(path);
        return _files.ContainsKey(key)
            || _files.Any(x => Path.GetDirectoryName(x.Key) == key);
    }

    public byte[] ReadBytesFromFile(string path)
    {
        var key = CreatePathKey(path);
        if (!_files.TryGetValue(key, out var fileContext))
        {
            throw new FileNotFoundException(path);
        }

        return fileContext.Content;
    }

    public string ReadTextFromFile(string path)
    {
        var key = CreatePathKey(path);
        if (!_files.TryGetValue(key, out var fileContext))
        {
            throw new FileNotFoundException(path);
        }

        return Encoding.Default.GetString(fileContext.Content);
    }

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(ReadTextFromFile(path));
    }

    public void WriteTextToFile(string path, string content)
    {
        var key = CreatePathKey(path);
        _files.Add(key, new FileContext(path, Encoding.Default.GetBytes(content)));
    }

    private string CreatePathKey(string path)
        => _ignoreCase ? path.ToLower() : path;

    public ICollection<FileContext> Files => _files.Values;
}
