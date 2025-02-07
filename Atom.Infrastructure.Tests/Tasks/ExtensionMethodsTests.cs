using Genius.Atom.Infrastructure.Tasks;
using Genius.Atom.Infrastructure.TestingUtil;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Tests;

public sealed class ExtensionMethodsTests
{
    [Fact]
    public void RunAndForget_HandlesExceptionOccurredInTask()
    {
        // Arrange
        var logger = new FakeLogger<Task>();
        using var serviceProvider = new FakeServiceProvider();
        Module.Initialize(serviceProvider);
        serviceProvider.RegisterInstance<ILogger<Task>>(logger);
        var task = Task.Run(() =>
        {
            throw new InvalidOperationException();
        });

        // Act
        task.RunAndForget();

        // Verify
        SpinWait.SpinUntil(() => logger.Logs.Count > 0, 10000);
        Assert.Single(logger.Logs);
        var log = logger.Logs.First();
        Assert.Equal(LogLevel.Error, log.LogLevel);
        Assert.IsType<InvalidOperationException>(log.Exception);
    }
}
