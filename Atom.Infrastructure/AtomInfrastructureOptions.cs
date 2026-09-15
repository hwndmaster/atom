using Genius.Atom.Infrastructure.Logging;

namespace Genius.Atom.Infrastructure;

/// <summary>
/// Opt-in behaviour for <see cref="Module.Configure(Microsoft.Extensions.DependencyInjection.IServiceCollection, Microsoft.Extensions.Configuration.IConfiguration?, Action{AtomInfrastructureOptions}?)"/>.
/// Every default matches how Atom behaved before the option existed, so leaving this alone changes
/// nothing for an application that already works.
/// </summary>
public sealed class AtomInfrastructureOptions
{
    /// <summary>
    /// Whether Serilog is added next to the host's own logger providers, or replaces them.
    /// Defaults to <see cref="AtomLoggingMode.Additive"/>.
    /// </summary>
    public AtomLoggingMode LoggingMode { get; set; } = AtomLoggingMode.Additive;
}
