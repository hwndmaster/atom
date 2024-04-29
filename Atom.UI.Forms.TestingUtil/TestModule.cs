using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.TestingUtil.Events;

namespace Genius.Atom.UI.Forms.TestingUtil;

public static class TestModule
{
    private static bool _isInitialized;
    private static TestServiceProvider _serviceProvider = new();

    public static void Initialize()
    {
        if (_isInitialized)
            return;

        _serviceProvider.AddSingleton<IEventBus, TestEventBus>(isTestImplementation: true);
        _serviceProvider.AddSingleton<IWpfApplication, TestWpfApplication>(isTestImplementation: true);
        _serviceProvider.RegisterInstance(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());

        Module.Initialize(_serviceProvider);
        _isInitialized = true;
    }

    public static TestServiceProvider ServiceProvider => _serviceProvider;
}
