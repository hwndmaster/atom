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
    ///   Returns directory information.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    DirectoryDetails GetDirectoryDetails(string path);

    /// <summary>
    ///   Returns directory size.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <param name="recursive">A flag indicating whether to perform a recursive directory size determination or not.</param>
    long GetDirectorySize(string path, bool recursive);

    /// <summary>
    ///   Returns file information.
    /// </summary>
    /// <param name="fullPath">The full path to the file.</param>
    FileDetails GetFileDetails(string fullPath);

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
    Task<string> ReadTextFromFileAsync(string path, CancellationToken? cancellationToken = default);

    /// <inheritdoc cref="File.ReadAllTextAsync(string, Encoding, CancellationToken)"/>
    Task<string> ReadTextFromFileAsync(string path, Encoding encoding, CancellationToken? cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllText(string, string?)"/>
    void WriteTextToFile(string path, string content);

    /// <inheritdoc cref="File.WriteAllText(string, string?, Encoding)"/>
    void WriteTextToFile(string path, string content, Encoding encoding);

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, Encoding, CancellationToken)"/>
    Task WriteTextToFileAsync(string path, string content, Encoding encoding, CancellationToken? cancellationToken = default);
}
