global using System.Windows;
global using Genius.Atom.Infrastructure;

using System.Diagnostics.CodeAnalysis;
using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.UI.Forms.Controls.AutoGrid.Builders;
using Genius.Atom.UI.Forms.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Genius.Atom.UI.Forms;

[ExcludeFromCodeCoverage]
public static class Module
{
    private static IServiceProvider? _serviceProvider;
    internal static IServiceProvider ServiceProvider => _serviceProvider!;

    public static IConfiguration Configure(IServiceCollection services, Application application, bool enableSerilog = true)
    {
        var config = LoadConfiguration(services);

        ConfigureLogging(services, config, enableSerilog);

        // View Models:
        services.AddTransient<ILogsTabViewModel, LogsTabViewModel>();

        // AutoGridBuilder:
        services.AddTransient(typeof(IAutoGridContextBuilder<,>), typeof(AutoGridContextBuilder<,>));
        services.AddTransient(typeof(AutoGridContextBuilderColumns<,>));
        services.AddTransient<DefaultAutoGridBuilder>();

        // Misc:
        services.AddTransient<IAtomViewModelFactory, ViewModelFactory>();
        services.AddTransient<IUserInteraction, UserInteraction>();
        services.AddSingleton<IUiDispatcher, UiDispatcher>();
        services.AddSingleton<IWpfApplication>(new WpfApplication(application));

        // Third-party:
        services.AddTransient<IDialogCoordinator, DialogCoordinator>();

        return config;
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.NotNull(message: "Call Genius.Atom.UI.Forms.Module.Initialize(serviceProvider) in your application initialization.");

        serviceProvider
            .GetService<Microsoft.Extensions.Logging.ILoggerFactory>()
            .NotNull()
            .AddProvider(new EventBasedLoggerProvider(serviceProvider.GetService<IEventBus>().NotNull()));
    }

    private static IConfiguration LoadConfiguration(IServiceCollection serviceCollection)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true);
        IConfiguration config = builder.Build();
        serviceCollection.AddSingleton<IConfiguration>(config);
        return config;
    }

    private static void ConfigureLogging(IServiceCollection services, IConfiguration configuration, bool enableSerilog)
    {
        if (enableSerilog)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithThreadId()
                .Enrich.WithComputed("SourceContextName", "Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)")
                .CreateLogger();
        }

        // Framework:
        services.AddLogging(x =>
        {
            if (enableSerilog)
            {
                x.AddSerilog();
            }
        });
    }
}
