global using System.Windows;
global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.UI.Forms.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms;

[ExcludeFromCodeCoverage]
public static class Module
{
    internal static IServiceProvider ServiceProvider { get; private set; }

    public static void Configure(IServiceCollection services)
    {
        // View Models:
        services.AddTransient<ILogsTabViewModel, LogsTabViewModel>();

        // Misc:
        services.AddTransient<IAtomViewModelFactory, ViewModelFactory>();
        services.AddTransient<IUserInteraction, UserInteraction>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;

        serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()!
            .AddProvider(new EventBasedLoggerProvider(serviceProvider.GetService<IEventBus>()!));
    }
}
