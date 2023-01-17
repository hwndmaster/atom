using System.Text;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public sealed class TestFileService : IFileService
{
    private readonly Dictionary<string, byte[]> _files = new();
    private readonly HashSet<string> _folders = new();
    private readonly bool _ignoreCase;

    public TestFileService(bool ignoreCase = true)
    {
        _ignoreCase = ignoreCase;
    }

    public void AddFile(string path, byte[]? content = null)
    {
        path = NormalizePath(path);
        _files.Add(path, content ?? Array.Empty<byte>());
        _folders.Add(Path.GetDirectoryName(path).NotNull());
    }

    public void EnsureDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        path = NormalizePath(path);
        if (!_folders.Contains(path))
        {
            _folders.Add(path);
        }
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
    {
        return _files
            .Where(x => path.Equals(Path.GetDirectoryName(x.Key), _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            .Select(x => x.Key);
    }

    public bool FileExists(string path)
    {
        path = NormalizePath(path);
        return _files.ContainsKey(path);
    }

    public bool IsDirectory(string path)
    {
        return !FileExists(path);
    }

    public Stream OpenRead(string path)
    {
        path = NormalizePath(path);
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException(path);
        }

        return new MemoryStream(content);
    }

    public Stream OpenReadNoLock(string path)
        => OpenRead(path);

    public bool PathExists(string path)
    {
        path = NormalizePath(path);
        return _files.ContainsKey(path)
            || _files.Any(x => Path.GetDirectoryName(x.Key) == path);
    }

    public byte[] ReadBytesFromFile(string path)
    {
        path = NormalizePath(path);
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException(path);
        }

        return content;
    }

    public string ReadTextFromFile(string path)
    {
        path = NormalizePath(path);
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException(path);
        }

        return Encoding.Default.GetString(content);
    }

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken)
    {
        return Task.FromResult(ReadTextFromFile(path));
    }

    public void WriteTextToFile(string path, string content)
    {
        path = NormalizePath(path);
        _files.Add(path, Encoding.Default.GetBytes(content));
    }

    private string NormalizePath(string path)
        => _ignoreCase ? path.ToLower() : path;

    public IReadOnlyDictionary<string, byte[]> CurrentFiles => _files;
}
