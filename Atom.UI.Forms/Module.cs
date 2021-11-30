global using System.Windows;
global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms;

[ExcludeFromCodeCoverage]
public static class Module
{
    public static void Configure(IServiceCollection services)
    {
        services.AddTransient<IViewModelFactory, ViewModelFactory>();
        services.AddTransient<IUserInteraction, UserInteraction>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()!
            .AddProvider(new EventBasedLoggerProvider(serviceProvider.GetService<IEventBus>()!));
    }
}
