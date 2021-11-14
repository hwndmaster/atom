using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileService
{
    bool FileExists(string path);
    byte[] ReadBytesFromFile(string path);
    string ReadTextFromFile(string path);
    void WriteTextToFile(string path, string content);
}

[ExcludeFromCodeCoverage]
internal sealed class FileService : IFileService
{
    public bool FileExists(string path)
        => File.Exists(path);

    public byte[] ReadBytesFromFile(string path)
        => File.ReadAllBytes(path);

    public string ReadTextFromFile(string path)
        => File.ReadAllText(path);

    public void WriteTextToFile(string path, string content)
        => File.WriteAllText(path, content, System.Text.Encoding.UTF8);
}
