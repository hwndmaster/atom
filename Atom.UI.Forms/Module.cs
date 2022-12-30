global using System.Windows;
global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider => _serviceProvider!;

    public static void Configure(IServiceCollection services, Application application)
    {
        // View Models:
        services.AddTransient<ILogsTabViewModel, LogsTabViewModel>();

        // AutoGridBuilder:
        services.AddTransient(typeof(IAutoGridContextBuilder<>), typeof(AutoGridContextBuilder<>));
        services.AddTransient(typeof(AutoGridContextBuilderColumns<>));
        services.AddTransient<DefaultAutoGridBuilder>();

        // Misc:
        services.AddTransient<IAtomViewModelFactory, ViewModelFactory>();
        services.AddTransient<IUserInteraction, UserInteraction>();
        services.AddSingleton<IUiDispatcher>(new UiDispatcher(application));

        // Third-party:
        services.AddTransient<IDialogCoordinator, DialogCoordinator>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull(message: "Call Genius.Atom.UI.Forms.Module.Initialize(serviceProvider) in your application initialization.");

        serviceProvider
            .GetService<Microsoft.Extensions.Logging.ILoggerFactory>()
            .NotNull()
            .AddProvider(new EventBasedLoggerProvider(serviceProvider.GetService<IEventBus>().NotNull()));
    }
}
