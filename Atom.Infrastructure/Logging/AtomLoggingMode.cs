namespace Genius.Atom.Infrastructure.Logging;

/// <summary>
/// How Atom's Serilog logging relates to the logger providers the .NET host has already registered.
/// </summary>
public enum AtomLoggingMode
{
    /// <summary>
    /// Serilog is registered alongside whatever providers are already present, the generic host's
    /// built-in Console, Debug and EventSource ones included. This is how Atom has always behaved and
    /// stays the default, so applications written before the other mode existed keep their output.
    /// <para>
    /// In a web application it means every line reaches the console twice: once from the host's console
    /// provider and once from Serilog's console sink.
    /// </para>
    /// </summary>
    Additive = 0,

    /// <summary>
    /// Serilog owns the output: the generic host's built-in providers are removed, leaving Serilog's own
    /// sinks as the only console writer.
    /// <para>
    /// Only those specific providers go. Anything the application registered itself — an OpenTelemetry
    /// provider, for instance — is deliberately kept, which is why this is not
    /// <c>ILoggingBuilder.ClearProviders()</c>: that would discard those too, and would do so or not
    /// depending on the order in which the application happened to call Atom.
    /// </para>
    /// </summary>
    ReplaceHostDefaults = 1,
}
