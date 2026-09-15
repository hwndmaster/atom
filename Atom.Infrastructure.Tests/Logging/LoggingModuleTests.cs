using Genius.Atom.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Tests.Logging;

public sealed class LoggingModuleTests
{
    [Fact]
    public void RemoveHostDefaultLoggerProviders__Removes_the_hosts_own_providers()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Debug.DebugLoggerProvider>();

        LoggingModule.RemoveHostDefaultLoggerProviders(services);

        Assert.Empty(services.Where(x => x.ServiceType == typeof(ILoggerProvider)));
    }

    /// <summary>
    /// The reason this is not ClearProviders(): an application that registered its own provider before
    /// calling Atom — an OpenTelemetry one feeding a dashboard, say — must keep it.
    /// </summary>
    [Fact]
    public void RemoveHostDefaultLoggerProviders__Keeps_providers_the_application_registered()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();
        services.AddSingleton<ILoggerProvider, ApplicationLoggerProvider>();

        LoggingModule.RemoveHostDefaultLoggerProviders(services);

        var survivor = Assert.Single(services.Where(x => x.ServiceType == typeof(ILoggerProvider)));
        Assert.Equal(typeof(ApplicationLoggerProvider), survivor.ImplementationType);
    }

    [Fact]
    public void Configure__WhenReplaceHostDefaults__Removes_the_hosts_own_providers()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();

        LoggingModule.Configure(services, mode: AtomLoggingMode.ReplaceHostDefaults);

        Assert.DoesNotContain(services, x => x.ImplementationType == typeof(Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider));
    }

    [Fact]
    public void Configure__ByDefault__Leaves_the_hosts_own_providers_alone()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();

        LoggingModule.Configure(services);

        Assert.Contains(services, x => x.ImplementationType == typeof(Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider));
    }

    /// <summary>
    /// Removing the host's providers with no Serilog to take their place would leave the application
    /// with no logging output at all, so the mode is ignored in that combination.
    /// </summary>
    [Fact]
    public void Configure__WhenSerilogExcluded__Ignores_the_mode()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILoggerProvider, Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider>();

        LoggingModule.Configure(services, includeSerilog: false, mode: AtomLoggingMode.ReplaceHostDefaults);

        Assert.Contains(services, x => x.ImplementationType == typeof(Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider));
    }

    private sealed class ApplicationLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => throw new NotSupportedException();
        public void Dispose() { }
    }
}
