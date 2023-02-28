using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Genius.Atom.Infrastructure.Io;

[ExcludeFromCodeCoverage]
internal sealed class FileService : IFileService
{
    public void CopyFile(string sourceFileName, string destFileName)
        => File.Copy(sourceFileName, destFileName);

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
        => File.Copy(sourceFileName, destFileName, overwrite);

    public Stream CreateFile(string path)
        => File.Create(path);

    public StreamWriter CreateTextFile(string path)
        => File.CreateText(path);

    public void DeleteDirectory(string path)
        => Directory.Delete(path);

    public void DeleteDirectory(string path, bool recursive)
        => Directory.Delete(path, recursive);

    public void DeleteFile(string path)
        => File.Delete(path);

    public void EnsureDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public IEnumerable<string> EnumerateDirectories(string path)
        => Directory.EnumerateDirectories(path);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        => Directory.EnumerateDirectories(path, searchPattern);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions options)
        => Directory.EnumerateDirectories(path, searchPattern, options);

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        => Directory.EnumerateDirectories(path, searchPattern, searchOption);

    public IEnumerable<string> EnumerateFiles(string path)
        => Directory.Exists(path) ? Directory.EnumerateFiles(path) : Array.Empty<string>();

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, searchPattern)
            : Array.Empty<string>();

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
        => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, searchPattern, options)
            : Array.Empty<string>();

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.Exists(path)
            ? Directory.EnumerateFiles(path, searchPattern, searchOption)
            : Array.Empty<string>();

    public bool FileExists(string path)
        => File.Exists(path);

    public DirectoryDetails GetDirectoryDetails(string path)
        => new(path, new DirectoryInfo(path), this);

    public FileDetails GetFileDetails(string fullPath)
        => new(fullPath, new FileInfo(fullPath), this);

    public bool IsDirectory(string path)
    {
        try
        {
            if (!Path.Exists(path))
            {
                return false;
            }

            var pathAttr = File.GetAttributes(path);
            return pathAttr.HasFlag(FileAttributes.Directory);
        }
        catch
        {
            return false;
        }
    }

    public void MoveFile(string sourceFileName, string destFileName)
        => File.Move(sourceFileName, destFileName);

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
        => File.Move(sourceFileName, destFileName, overwrite);

    public Stream OpenRead(string path)
        => File.OpenRead(path);

    public Stream OpenReadNoLock(string path)
        => File.Open(path, new FileStreamOptions {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous,
            Share = FileShare.ReadWrite
        });

    public bool PathExists(string path)
        => Path.Exists(path);

    public byte[] ReadBytesFromFile(string path)
        => File.ReadAllBytes(path);

    public Task<byte[]> ReadBytesFromFileAsync(string path, CancellationToken? cancellationToken = default)
        => File.ReadAllBytesAsync(path, cancellationToken ?? default);

    public string ReadTextFromFile(string path)
        => File.ReadAllText(path);

    public string ReadTextFromFile(string path, Encoding encoding)
        => File.ReadAllText(path, encoding);

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken? cancellationToken = default)
        => File.ReadAllTextAsync(path, cancellationToken ?? default);

    public Task<string> ReadTextFromFileAsync(string path, Encoding encoding, CancellationToken? cancellationToken = default)
        => File.ReadAllTextAsync(path, encoding, cancellationToken ?? default);

    public void WriteTextToFile(string path, string content)
        => File.WriteAllText(path, content);

    public void WriteTextToFile(string path, string content, Encoding encoding)
        => File.WriteAllText(path, content, encoding);

    public Task WriteTextToFileAsync(string path, string content, Encoding encoding, CancellationToken? cancellationToken = default)
        => File.WriteAllTextAsync(path, content, encoding, cancellationToken ?? default);
}
