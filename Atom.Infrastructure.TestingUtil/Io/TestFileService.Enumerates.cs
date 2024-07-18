using System.Text.RegularExpressions;
using Genius.Atom.Infrastructure.Io;

namespace Genius.Atom.Infrastructure.TestingUtil.Io;

public sealed partial class TestFileService : IFileService
{
    public IEnumerable<string> EnumerateDirectories(string path)
        => EnumerateDirectories(path, "*", new EnumerationOptions());

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
        => EnumerateDirectories(path, searchPattern, new EnumerationOptions());

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        => EnumerateDirectories(path, searchPattern, new EnumerationOptions
        {
            RecurseSubdirectories = searchOption == SearchOption.AllDirectories
        });

    public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions options)
        => EnumerateFilesOrDirectories(_dirs.Values, path, searchPattern, options.NotNull()).Select(x => x.FullName);

    public IEnumerable<string> EnumerateFiles(string path)
        => EnumerateFiles(path, "*", new EnumerationOptions());

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        => EnumerateFiles(path, searchPattern, new EnumerationOptions());

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions options)
        => EnumerateFilesOrDirectories(_files.Values, path, searchPattern, options.NotNull()).Select(x => x.FullName);

    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        => EnumerateFiles(path, searchPattern, new EnumerationOptions
        {
            RecurseSubdirectories = searchOption == SearchOption.AllDirectories
        });

    private IEnumerable<EntityContext> EnumerateFilesOrDirectories(IEnumerable<EntityContext> collection, string path, string? searchPattern, EnumerationOptions? options)
    {
        if (options.MaxRecursionDepth is not int.MaxValue or 0)
            throw new NotSupportedException("Custom MaxRecursionDepth values are not supported.");
        if (options.MatchType == MatchType.Win32)
            throw new NotSupportedException("MatchType.Win32 is not supported.");
        if (options.ReturnSpecialDirectories)
            throw new NotSupportedException("ReturnSpecialDirectories is not supported.");

        var useSearchPattern = !string.IsNullOrEmpty(searchPattern)
            && !searchPattern.Equals("*")
            && !searchPattern.Equals("*.*");
        var regex = useSearchPattern
            ? new Regex("^" + Regex.Escape(searchPattern!).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.Singleline | (_ignoreCase ? RegexOptions.IgnoreCase : 0))
            : null;

        return collection
            .Where(x =>
            {
                if (options.AttributesToSkip != 0
                    && (x.GenericDetails.Attributes & options.AttributesToSkip) == x.GenericDetails.Attributes)
                {
                    return false;
                }

                var ignoreCase = options.MatchCasing switch
                {
                    MatchCasing.CaseSensitive => false,
                    MatchCasing.CaseInsensitive => true,
                    _ => _ignoreCase
                };

                if (useSearchPattern)
                {
                    var directoryName = Path.GetFileName(x.FullName);
                    if (directoryName is null)
                        return false;
                    if (!regex!.IsMatch(directoryName))
                        return false;
                }

                var pathComparer = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                var pathToThisEntity = Path.GetDirectoryName(x.FullName).NotNull();
                if (options.RecurseSubdirectories)
                {
                    return pathToThisEntity.StartsWith(path, pathComparer);
                }

                return path.Equals(pathToThisEntity, pathComparer);
            });
    }
}
