using System.Text;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public sealed class TestFileService : IFileService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public void AddFile(string path, byte[] content)
    {
        _files.Add(path, content);
    }

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
    {
        return _files
            .Where(x => path.Equals(Path.GetDirectoryName(x.Key), StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key);
    }

    public bool FileExists(string path)
    {
        return _files.ContainsKey(path);
    }

    public Stream OpenRead(string path)
    {
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException(path);
        }

        return new MemoryStream(content);
    }

    public byte[] ReadBytesFromFile(string path)
    {
        if (!_files.TryGetValue(path, out var content))
        {
            throw new FileNotFoundException(path);
        }

        return content;
    }

    public string ReadTextFromFile(string path)
    {
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
        _files.Add(path, Encoding.Default.GetBytes(content));
    }
}
