using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileService
{
    /// <inheritdoc cref="Directory.EnumerateFiles(string, string, EnumerationOptions)"/>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options);

    /// <inheritdoc cref="File.FileExists(string)"/>
    bool FileExists(string path);

    /// <inheritdoc cref="File.OpenRead(string)"/>
    Stream OpenRead(string path);

    /// <inheritdoc cref="File.ReadAllBytes(string)"/>
    byte[] ReadBytesFromFile(string path);

    /// <inheritdoc cref="File.ReadAllText(string)"/>
    string ReadTextFromFile(string path);

    /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)"/>
    Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken);

    /// <inheritdoc cref="File.WriteAllText(string, string, System.Text.Encoding)"/>
    void WriteTextToFile(string path, string content);
}

[ExcludeFromCodeCoverage]
internal sealed class FileService : IFileService
{
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
        => Directory.EnumerateFiles(path, searchPattern, options);

    public bool FileExists(string path)
        => File.Exists(path);

    public Stream OpenRead(string path)
        => File.OpenRead(path);

    public byte[] ReadBytesFromFile(string path)
        => File.ReadAllBytes(path);

    public string ReadTextFromFile(string path)
        => File.ReadAllText(path);

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(path, cancellationToken);

    public void WriteTextToFile(string path, string content)
        => File.WriteAllText(path, content, System.Text.Encoding.UTF8);
}
