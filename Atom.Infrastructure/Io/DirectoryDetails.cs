namespace Genius.Atom.Infrastructure.Io;

/// <summary>
///   Represents the details of a directory in the file system.
/// </summary>
public sealed class DirectoryDetails : FileSystemDetails
{
    internal DirectoryDetails(string path,
        FileAttributes attributes,
        DateTime creationTime,
        DateTime creationTimeUtc,
        DateTime lastAccessTime,
        DateTime lastAccessTimeUtc,
        DateTime lastWriteTime,
        DateTime lastWriteTimeUtc,
        IFileService fileService)
        : base(path,
            attributes,
            creationTime,
            creationTimeUtc,
            lastAccessTime,
            lastAccessTimeUtc,
            lastWriteTime,
            lastWriteTimeUtc,
            fileService)
    {
        Name = Path.GetFileName(path).NotNull();
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DirectoryDetails"/> class.
    /// </summary>
    /// <param name="path">The path to the directory.</param>
    /// <param name="info">The directory detailed information.</param>
    /// <param name="fileService">The file service which is used to check for path existence and for calculating the directory size.</param>
    public DirectoryDetails(string path, DirectoryInfo info, IFileService fileService)
        : base(path, info, fileService)
    {
        Name = Path.GetFileName(path).NotNull();
    }

    /// <summary>
    ///   Calculates the size of the directory, in bytes.
    /// </summary>
    public long CalculateDirectorySize()
    {
        return FileService
            .EnumerateFiles(FullPath, "*", SearchOption.AllDirectories)
            .Select(x => FileService.GetFileDetails(x).Length)
            .Sum();
    }

    /// <summary>
    ///   Gets a value indicating whether the directory exists.
    /// </summary>
    public override bool Exists => FileService.PathExists(Name);

    /// <inheritdoc />
    public override string Name { get; protected set; }
}
