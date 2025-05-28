using Genius.Atom.Infrastructure.Events;
using Genius.Atom.Infrastructure.Logging;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.Infrastructure.TestingUtil.Events;
using Genius.Atom.Infrastructure.Threading;

namespace Genius.Atom.UI.Forms.TestingUtil;

public static class TestModule
{
    private static readonly FakeServiceProvider _serviceProvider = new();
    private static bool _isInitialized;

    public static void Initialize(Action<FakeServiceProvider>? supplementalInitialization = null)
    {
        if (_isInitialized)
            return;

        _serviceProvider.AddSingleton<EventBasedLoggerProvider, EventBasedLoggerProvider>();
        _serviceProvider.AddSingleton<IEventBus, FakeEventBus>(isTestImplementation: true);
        _serviceProvider.AddSingleton<IWpfApplication, TestWpfApplication>(isTestImplementation: true);
        _serviceProvider.RegisterInstance(A.Fake<Microsoft.Extensions.Logging.ILoggerFactory>());
        _serviceProvider.RegisterInstance(new JoinableTaskHelper());

        if (supplementalInitialization is not null)
            supplementalInitialization(_serviceProvider);

        Module.Initialize(_serviceProvider);
        _isInitialized = true;
    }

    public static FakeServiceProvider ServiceProvider => _serviceProvider;
}
