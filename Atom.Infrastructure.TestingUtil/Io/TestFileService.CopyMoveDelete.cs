using System.Text;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public sealed partial class TestFileService : IFileService
{
    public void CopyFile(string sourceFileName, string destFileName)
        => CopyOrMoveFile(sourceFileName, destFileName, overwrite: false, copy: true);

    public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
        => CopyOrMoveFile(sourceFileName, destFileName, overwrite, copy: true);

    public void DeleteFile(string path)
    {
        Guard.NotNullOrWhitespace(path);

        var key = CreatePathKey(path);
        _files.Remove(key);
    }

    public void DeleteDirectory(string path)
        => DeleteDirectory(path, false);

    public void DeleteDirectory(string path, bool recursive)
    {
        Guard.NotNullOrWhitespace(path);

        var key = CreatePathKey(path);

        foreach (var dir in _dirs.ToArray())
        {
            if (dir.Key.StartsWith(key + "\\"))
            {
                if (!recursive)
                    throw new IOException("Cannot delete the directory, as it isn't empty.");
                _dirs.Remove(dir.Key);
            }
        }
        foreach (var file in _files)
        {
            if (file.Key.StartsWith(key + "\\"))
            {
                if (!recursive)
                    throw new IOException("Cannot delete the directory, as it isn't empty.");
                _files.Remove(file.Key);
            }
        }

        _dirs.Remove(key);
    }

    public void MoveFile(string sourceFileName, string destFileName)
        => CopyOrMoveFile(sourceFileName, destFileName, overwrite: false, copy: false);

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
        => CopyOrMoveFile(sourceFileName, destFileName, overwrite, copy: false);

    private void CopyOrMoveFile(string sourceFileName, string destFileName, bool overwrite, bool copy)
    {
        Guard.NotNullOrWhitespace(sourceFileName);
        Guard.NotNullOrWhitespace(destFileName);

        var oldPathKey = CreatePathKey(sourceFileName);
        var newPathKey = CreatePathKey(destFileName);

        if (!_files.TryGetValue(oldPathKey, out var context))
        {
            throw new KeyNotFoundException($"The file with path '{sourceFileName}' wasn't added beforehand.");
        }

        if (PathExists(destFileName))
        {
            if (overwrite)
            {
                _files.Remove(newPathKey);
            }
            else
            {
                throw new IOException($"File '{destFileName}' already exists.");
            }
        }

        if (!copy)
        {
            _files.Remove(oldPathKey);
        }

        _files.Add(newPathKey, context with { FullName = destFileName });
    }
}
