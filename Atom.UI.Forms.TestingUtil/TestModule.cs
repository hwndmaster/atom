using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.Infrastructure.TestingUtil.Events;

namespace Genius.Atom.UI.Forms.TestingUtil;

public static class TestModule
{
    private static bool _isInitialized;
    private static FakeServiceProvider _serviceProvider = new();

    public static void Initialize()
    {
        if (_isInitialized)
            return;

        _serviceProvider.AddSingleton<EventBasedLoggerProvider, EventBasedLoggerProvider>();
        _serviceProvider.AddSingleton<IEventBus, FakeEventBus>(isTestImplementation: true);
        _serviceProvider.AddSingleton<IWpfApplication, TestWpfApplication>(isTestImplementation: true);
        _serviceProvider.RegisterInstance(A.Fake<Microsoft.Extensions.Logging.ILoggerFactory>());

        Module.Initialize(_serviceProvider);
        _isInitialized = true;
    }

    public static FakeServiceProvider ServiceProvider => _serviceProvider;
}
