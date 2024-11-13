using System.Diagnostics.CodeAnalysis;

namespace Genius.Atom.Infrastructure.Io;

public sealed class FileDetails : FileSystemDetails
{
    internal FileDetails(string filePath,
        long length,
        FileAttributes attributes,
        DateTime creationTime,
        DateTime creationTimeUtc,
        DateTime lastAccessTime,
        DateTime lastAccessTimeUtc,
        DateTime lastWriteTime,
        DateTime lastWriteTimeUtc,
        IFileService fileService)
        : base(filePath,
            attributes,
            creationTime,
            creationTimeUtc,
            lastAccessTime,
            lastAccessTimeUtc,
            lastWriteTime,
            lastWriteTimeUtc,
            fileService)
    {
        Init(filePath, length);
    }

    public FileDetails(string filePath, FileInfo info, IFileService fileService)
        : base(filePath, info, fileService)
    {
        Init(filePath, info.Length);
    }

    [MemberNotNull(nameof(Name), nameof(Length))]
    private void Init(string filePath, long length)
    {
        Name = Path.GetFileName(filePath).NotNull();
        DirectoryName = Path.GetDirectoryName(filePath);
        Length = length;
    }

    public override bool Exists => FileService.FileExists(Name);

    public override string Name { get; protected set; }

    public bool IsReadOnly => Attributes.HasFlag(FileAttributes.ReadOnly);
    public string? DirectoryName { get; private set; }
    public DirectoryDetails? Directory => DirectoryName is not null
        ? new DirectoryDetails(DirectoryName, new DirectoryInfo(DirectoryName), FileService)
        : null;
    public long Length { get; private set; }
}
