using System.Globalization;

namespace Genius.Atom.Web.Telemetry.Observability;

/// <summary>
/// Collects the application-specific values that go into the startup summary, next to the ones Atom
/// knows about itself. Values are rendered in the order they are added.
/// </summary>
public sealed class StartupSummary
{
    private readonly List<KeyValuePair<string, string>> _values = [];

    internal IReadOnlyList<KeyValuePair<string, string>> Values => _values;

    /// <summary>
    /// Adds a labelled value to the summary, e.g. <c>Add("Database", dbPath)</c>.
    /// </summary>
    /// <param name="name">Label shown in the log line.</param>
    /// <param name="value">Value; null and empty render as "(not set)".</param>
    /// <returns>This instance, for chaining.</returns>
    public StartupSummary Add(string name, object? value)
    {
        Guard.NotNullOrWhitespace(name);

        var text = value switch
        {
            null => "(not set)",
            string s when string.IsNullOrWhiteSpace(s) => "(not set)",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "(not set)",
        };

        _values.Add(new KeyValuePair<string, string>(name, text));
        return this;
    }

    /// <summary>
    /// Adds the size and existence of a file, which for these apps is almost always the SQLite database.
    /// </summary>
    /// <param name="name">Label shown in the log line.</param>
    /// <param name="path">Path to report on.</param>
    /// <returns>This instance, for chaining.</returns>
    public StartupSummary AddFile(string name, string path)
    {
        Guard.NotNullOrWhitespace(name);

        if (string.IsNullOrWhiteSpace(path))
        {
            return Add(name, null);
        }

        if (!File.Exists(path))
        {
            return Add(name, $"{path} (does not exist yet)");
        }

        try
        {
            var sizeKb = new FileInfo(path).Length / 1024;
            return Add(name, $"{path} ({sizeKb.ToString(CultureInfo.InvariantCulture)} KB)");
        }
        catch (IOException)
        {
            return Add(name, $"{path} (size unavailable)");
        }
    }
}
