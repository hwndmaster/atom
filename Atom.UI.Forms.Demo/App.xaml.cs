using System.Windows;
using Genius.Atom.UI.Forms.Demo.ViewModels;
using Genius.Atom.UI.Forms.Demo.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms.Demo;

public partial class App : Application
{
#pragma warning disable CS8618
    public static IServiceProvider ServiceProvider { get; private set; }
#pragma warning restore CS8618

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTransient<MainWindow>();
        serviceCollection.AddTransient<MainViewModel>();

        Infrastructure.Module.Configure(serviceCollection);
        UI.Forms.Module.Configure(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        //Infrastructure.Module.Initialize(ServiceProvider);
        UI.Forms.Module.Initialize(ServiceProvider);

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
