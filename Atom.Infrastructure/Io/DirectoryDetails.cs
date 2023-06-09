namespace Genius.Atom.Infrastructure.Io;

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

    public DirectoryDetails(string path, DirectoryInfo info, IFileService fileService)
        : base(path, info, fileService)
    {
        Name = Path.GetFileName(path).NotNull();
    }

    public long CalculateDirectorySize()
    {
        return _fileService
            .EnumerateFiles(FullPath, "*", SearchOption.AllDirectories)
            .Select(x => _fileService.GetFileDetails(x).Length)
            .Sum();
    }

    public override bool Exists => _fileService.PathExists(Name);

    public override string Name { get; protected set; }
}
