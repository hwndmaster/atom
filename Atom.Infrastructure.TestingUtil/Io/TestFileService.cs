using System.Text;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public abstract record EntityContext(string FullName, FileAttributes Attributes);
public record FileContext(string FullName, byte[] Content, FileAttributes Attributes) : EntityContext(FullName, Attributes);
public record DirectoryContext(string FullName, FileAttributes Attributes) : EntityContext(FullName, Attributes);

public sealed partial class TestFileService : IFileService
{
    private readonly Dictionary<string, FileContext> _files = new();
    private readonly Dictionary<string, DirectoryContext> _dirs = new();
    private readonly bool _ignoreCase;

    public TestFileService(bool ignoreCase = true)
    {
        _ignoreCase = ignoreCase;
    }

    public void AddFile(string path, byte[]? content = null, FileAttributes? fileAttributes = null)
    {
        Guard.NotNullOrWhitespace(path);

        var key = CreatePathKey(path);
        _files[key] = new FileContext(path, content ?? Array.Empty<byte>(), fileAttributes ?? FileAttributes.Normal);

        AddDirectory(Path.GetDirectoryName(key).NotNull());
    }

    public void AddDirectory(string path, FileAttributes? fileAttributes = null)
    {
        Guard.NotNullOrWhitespace(path);

        if (_dirs.ContainsKey(path))
            return;

        _dirs.Add(path, new DirectoryContext(path, FileAttributes.Directory | (fileAttributes ?? 0)));
    }

    public Stream CreateFile(string path)
    {
        Guard.NotNullOrWhitespace(path);

        AddFile(path);
        return new MemoryStreamWrapper((content) =>
        {
            var key = CreatePathKey(path);
            _files[key] =_files[key] with { Content = content };
        });
    }

    public StreamWriter CreateTextFile(string path)
        => new(CreateFile(path));

    public void EnsureDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        AddDirectory(path);
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
            || _dirs.ContainsKey(key);
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

    public Task<byte[]> ReadBytesFromFileAsync(string path, CancellationToken? cancellationToken = null)
        => Task.FromResult(ReadBytesFromFile(path));

    public string ReadTextFromFile(string path)
        => ReadTextFromFile(path, Encoding.UTF8);

    public string ReadTextFromFile(string path, Encoding encoding)
    {
        var key = CreatePathKey(path);
        if (!_files.TryGetValue(key, out var fileContext))
        {
            throw new FileNotFoundException(path);
        }

        return encoding.GetString(fileContext.Content);
    }

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken? cancellationToken = default)
    {
        return Task.FromResult(ReadTextFromFile(path));
    }

    public Task<string> ReadTextFromFileAsync(string path, Encoding encoding, CancellationToken? cancellationToken = default)
    {
        return Task.FromResult(ReadTextFromFile(path, encoding));
    }

    public void WriteTextToFile(string path, string content)
        => WriteTextToFile(path, content, Encoding.UTF8);

    public void WriteTextToFile(string path, string content, Encoding encoding)
    {
        var key = CreatePathKey(path);
        _files.Add(key, new FileContext(path, encoding.GetBytes(content), FileAttributes.Normal));
    }

    public Task WriteTextToFileAsync(string path, string content, Encoding encoding, CancellationToken? cancellationToken = null)
    {
        WriteTextToFile(path, content, encoding);
        return Task.CompletedTask;
    }

    private string CreatePathKey(string path)
        => _ignoreCase ? path.ToLower() : path;

    public ICollection<DirectoryContext> Directories => _dirs.Values;
    public ICollection<FileContext> Files => _files.Values;
}
