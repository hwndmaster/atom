using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure.Io;

/// <summary>
///   Represents the details of a file or directory in the file system.
/// </summary>
public abstract class FileSystemDetails
{
    protected readonly IFileService FileService;

    protected FileSystemDetails(string fullPath,
        FileAttributes attributes,
        DateTime creationTime,
        DateTime creationTimeUtc,
        DateTime lastAccessTime,
        DateTime lastAccessTimeUtc,
        DateTime lastWriteTime,
        DateTime lastWriteTimeUtc,
        IFileService fileService)
    {
        FileService = fileService.NotNull();
        FullPath = fullPath.NotNull();
        Attributes = attributes;
        CreationTime = creationTime;
        CreationTimeUtc = creationTimeUtc;
        LastAccessTime = lastAccessTime;
        LastAccessTimeUtc = lastAccessTimeUtc;
        LastWriteTime = lastWriteTime;
        LastWriteTimeUtc = lastWriteTimeUtc;
    }

    protected FileSystemDetails(string fullPath, [NotNull] FileSystemInfo info, IFileService fileService)
        : this(fullPath,
            info.NotNull().Attributes,
            info.CreationTime,
            info.CreationTimeUtc,
            info.LastAccessTime,
            info.LastAccessTimeUtc,
            info.LastWriteTime,
            info.LastWriteTimeUtc,
            fileService)
    {
    }

    /// <summary>
    ///   Gets or sets the attributes for the current file or directory.
    /// </summary>
    public FileAttributes Attributes { get; }

    /// <summary>
    ///   Gets or sets the creation time of the current file or directory.
    /// </summary>
    public DateTime CreationTime { get; }

    /// <summary>
    ///   Gets or sets the creation time, in coordinated universal time (UTC), of the current file or directory.
    /// </summary>
    public DateTime CreationTimeUtc { get; }

    /// <summary>
    ///   Gets a value indicating whether the file or directory exists.
    /// </summary>
    /// <returns>
    ///   true if the file or directory exists; otherwise, false.
    /// </returns>
    public abstract bool Exists { get; }

    /// <summary>
    ///   Gets the extension part of the file name, including the leading dot . even if
    ///   it is the entire file name, or an empty string if no extension is present.
    /// </summary>
    public string Extension => Path.GetExtension(FullPath);

    /// <summary>
    ///   Gets the full path of the directory or file.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    ///   Gets or sets the time the current file or directory was last accessed.
    /// </summary>
    public DateTime LastAccessTime { get; }

    /// <summary>
    ///   Gets or sets the time, in coordinated universal time (UTC), that the current file or directory was last accessed.
    /// </summary>
    public DateTime LastAccessTimeUtc { get; }

    /// <summary>
    ///   Gets or sets the time when the current file or directory was last written to.
    /// </summary>
    public DateTime LastWriteTime { get; }

    /// <summary>
    ///   Gets or sets the time, in coordinated universal time (UTC), when the current file or directory was last written to.
    /// </summary>
    public DateTime LastWriteTimeUtc { get; }

    /// <summary>
    ///   For files, gets the name of the file. For directories, gets the name of the last
    ///   directory in the hierarchy if a hierarchy exists. Otherwise, the Name property
    ///   gets the name of the directory.
    /// </summary>
    public abstract string Name { get; protected set; }
}
