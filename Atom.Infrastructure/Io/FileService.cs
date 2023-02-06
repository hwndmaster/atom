using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Genius.Atom.Infrastructure.Io;

public interface IFileService
{
    /// <inheritdoc cref="File.Copy(string, string)"/>
    void CopyFile(string sourceFileName, string destFileName);

    /// <inheritdoc cref="File.Copy(string, string, bool)"/>
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);

    /// <inheritdoc cref="File.Create(string)"/>
    Stream CreateFile(string path);

    /// <inheritdoc cref="File.CreateText(string)"/>
    StreamWriter CreateTextFile(string path);

    /// <inheritdoc cref="File.Delete(string)"/>
    void DeleteFile(string path);

    /// <inheritdoc cref="Directory.Delete(string)"/>
    void DeleteDirectory(string path);

    /// <inheritdoc cref="Directory.Delete(string, bool)"/>
    void DeleteDirectory(string path, bool recursive);

    /// <summary>
    ///   Creates a directory at the specified <paramref name="path"/> if it was not exist.
    /// </summary>
    /// <param name="path">The directory path to check existence and create.</param>
    void EnsureDirectory(string? path);

    /// <inheritdoc cref="Directory.EnumerateDirectories(string)"/>
    IEnumerable<string> EnumerateDirectories(string path);

    /// <inheritdoc cref="Directory.EnumerateDirectories(string, string)"/>
    IEnumerable<string> EnumerateDirectories(string path, string searchPattern);

    /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, EnumerationOptions)"/>
    IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions options);

    /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, SearchOption)"/>
    IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

    /// <inheritdoc cref="Directory.EnumerateFiles(string)"/>
    IEnumerable<string> EnumerateFiles(string path);

    /// <inheritdoc cref="Directory.EnumerateFiles(string, string)"/>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern);

    /// <inheritdoc cref="Directory.EnumerateFiles(string, string, EnumerationOptions)"/>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options);

    /// <inheritdoc cref="Directory.EnumerateFiles(string, string, SearchOption)"/>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

    /// <inheritdoc cref="File.FileExists(string)"/>
    bool FileExists(string path);

    /// <summary>
    ///   Determines whether the specified <paramref name="path"/> is a directory or not.
    /// </summary>
    /// <param name="path">The path to a directory or file.</param>
    /// <returns>True if the specified path is pointing to a directory.</returns>
    bool IsDirectory(string path);

    /// <inheritdoc cref="File.Move(string, string)"/>
    void MoveFile(string sourceFileName, string destFileName);

    /// <inheritdoc cref="File.Move(string, string, bool)"/>
    void MoveFile(string sourceFileName, string destFileName, bool overwrite);

    /// <inheritdoc cref="File.OpenRead(string)"/>
    Stream OpenRead(string path);

    /// <summary>
    ///   Opens an existing file for reading without applying any lock,
    ///   which makes possible to read already locked files.
    /// </summary>
    /// <param name="path">The file to be opened for reading.</param>
    /// <returns>A read-only System.IO.FileStream on the specified path.</returns>
    Stream OpenReadNoLock(string path);

    /// <inheritdoc cref="Path.Exists(string?)"/>
    bool PathExists(string path);

    /// <inheritdoc cref="File.ReadAllBytes(string)"/>
    byte[] ReadBytesFromFile(string path);

    /// <inheritdoc cref="File.ReadAllBytesAsync(string, CancellationToken)"/>
    Task<byte[]> ReadBytesFromFileAsync(string path, CancellationToken? cancellationToken = default);

    /// <inheritdoc cref="File.ReadAllText(string)"/>
    string ReadTextFromFile(string path);

    /// <inheritdoc cref="File.ReadAllText(string, Encoding)"/>
    string ReadTextFromFile(string path, Encoding encoding);

    /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)"/>
    Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken);

    /// <inheritdoc cref="File.ReadAllTextAsync(string, Encoding, CancellationToken)"/>
    Task<string> ReadTextFromFileAsync(string path, Encoding encoding, CancellationToken cancellationToken);

    /// <inheritdoc cref="File.WriteAllText(string, string?)"/>
    void WriteTextToFile(string path, string content);

    /// <inheritdoc cref="File.WriteAllText(string, string?, Encoding)"/>
    void WriteTextToFile(string path, string content, Encoding encoding);

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, Encoding, CancellationToken)"/>
    Task WriteTextToFileAsync(string path, string content, Encoding encoding, CancellationToken? cancellationToken = default);
}

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
        => Directory.EnumerateFiles(path);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        => Directory.EnumerateFiles(path, searchPattern);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
        => Directory.EnumerateFiles(path, searchPattern, options);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.EnumerateFiles(path, searchPattern, searchOption);

    public bool FileExists(string path)
        => File.Exists(path);

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

    public Task<string> ReadTextFromFileAsync(string path, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(path, cancellationToken);

    public Task<string> ReadTextFromFileAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        => File.ReadAllTextAsync(path, encoding, cancellationToken);

    public void WriteTextToFile(string path, string content)
        => File.WriteAllText(path, content);

    public void WriteTextToFile(string path, string content, Encoding encoding)
        => File.WriteAllText(path, content, encoding);

    public Task WriteTextToFileAsync(string path, string content, Encoding encoding, CancellationToken? cancellationToken = default)
        => File.WriteAllTextAsync(path, content, encoding, cancellationToken ?? default);
}
