using System.Diagnostics;
using Genius.Atom.Infrastructure.Commands;
using Genius.Atom.Infrastructure.Tasks;
using Genius.Atom.Infrastructure.TestingUtil;
using Genius.Atom.Infrastructure.TestingUtil.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Infrastructure.Tests;

public sealed class CommandBusTests
{
    private readonly IServiceProvider _serviceProviderMock;
    private readonly FakeSynchronousScheduler _synchronousScheduler = new();
    private readonly TestLogger<CommandBus> _logger = new();
    private readonly CommandBus _sut;

    public CommandBusTests()
    {
        _serviceProviderMock = A.Fake<IServiceProvider>();
        var serviceScopeMock = A.Fake<IServiceScope>();
        A.CallTo(() => serviceScopeMock.ServiceProvider).Returns(_serviceProviderMock);
        var serviceScopeFactoryMock = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactoryMock.CreateScope())
            .Returns(serviceScopeMock);

        _sut = new CommandBus(serviceScopeFactoryMock, _synchronousScheduler, _logger);
    }

    [Fact]
    public async Task SendAsync_GivenSimpleMessage_ThenReturnsResult()
    {
        // Arrange
        var handlerType = typeof(ICommandHandler<DummyCommand>);
        DummyHandler? dummyHandler = null;
        A.CallTo(() => _serviceProviderMock.GetService(handlerType))
            .ReturnsLazily(() => dummyHandler = new DummyHandler(shouldThrowException: false));

        // Act
        await _sut.SendAsync(new DummyCommand());

        // Verify
        Assert.NotNull(dummyHandler);
        Assert.True(dummyHandler.ExecutionCompleted);
    }

    [Fact]
    public async Task SendAsync_GivenExchangeMessage_ThenReturnsResult()
    {
        // Arrange
        var handlerType = typeof(ICommandHandler<DummyExchangeCommand, Guid>);
        DummyExchangeHandler? dummyHandler = null;
        A.CallTo(() => _serviceProviderMock.GetService(handlerType))
            .ReturnsLazily(() => dummyHandler = new DummyExchangeHandler());

        // Act
        await _sut.SendAsync(new DummyExchangeCommand());

        // Verify
        Assert.NotNull(dummyHandler);
        Assert.True(dummyHandler.ExecutionCompleted);
    }

    [Fact]
    public async Task SendAsync_WhenException_ThenAddsToLogsAndReportsGenericException()
    {
        // Arrange
        var handlerType = typeof(ICommandHandler<DummyCommand>);
        DummyHandler? dummyHandler = null;
        A.CallTo(() => _serviceProviderMock.GetService(handlerType))
            .ReturnsLazily(() => dummyHandler = new DummyHandler(shouldThrowException: true));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await _sut.SendAsync(new DummyCommand()));

        // Verify
        Assert.NotNull(dummyHandler);
        Assert.False(dummyHandler.ExecutionCompleted);
        var log = _logger.Logs.Single();
        Assert.Equal(Microsoft.Extensions.Logging.LogLevel.Error, log.LogLevel);
        Assert.Equal("Bloody exception here!", log.Exception!.Message);
    }

    [Fact]
    public async Task SendAsync_WhenConcurrentMessagesArriving_InvokesThemSynchronously()
    {
        // Arrange
        const int tasksToRun = 5;
        var locker = new object();
        var handlerType = typeof(ICommandHandler<DummyCommand>);
        List<DummyHandler> dummyHandlers = [];
        A.CallTo(() => _serviceProviderMock.GetService(handlerType))
            .ReturnsLazily(() =>
            {
                DummyHandler handler;
                lock (locker)
                {
                    var delayTime = 5 + (tasksToRun - dummyHandlers.Count) * 5;
                    handler = new DummyHandler(shouldThrowException: false, delayTime);
                    dummyHandlers.Add(handler);
                }
                return handler;
            });

        // Act
        for (var i = 0; i < tasksToRun; i++)
            Task.Factory.StartNew(() => _sut.SendAsync(new DummyCommand())).RunAndForget();

        while (dummyHandlers.Count != tasksToRun
            || !dummyHandlers.TrueForAll(x => x.ExecutionCompleted))
        {
            await Task.Delay(5);
        }

        // Verify
        Assert.Equal(tasksToRun, dummyHandlers.Count);
        Assert.True(dummyHandlers.TrueForAll(x => x.ExecutionCompleted));
        var handlersOrdered = dummyHandlers.OrderBy(x => x.TimeStarted).ToArray();
        for (var i = 1; i < handlersOrdered.Length; i++)
        {
            Assert.True(handlersOrdered[i - 1].TimeFinished <= handlersOrdered[i].TimeStarted);
        }
    }

    class DummyCommand : ICommandMessage
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    class DummyHandler : ICommandHandler<DummyCommand>
    {
        private readonly bool _shouldThrowException;
        private readonly int _delayTime;

        public DummyHandler(bool shouldThrowException, int delayTime = 0)
        {
            _shouldThrowException = shouldThrowException;
            _delayTime = delayTime;
        }

        public async Task ProcessAsync(DummyCommand command)
        {
            if (_shouldThrowException)
                throw new InvalidOperationException("Bloody exception here!");

            TimeStarted = Stopwatch.GetTimestamp();

            if (_delayTime > 0)
                await Task.Delay(_delayTime);

            TimeFinished = Stopwatch.GetTimestamp();
            ExecutionCompleted = true;
        }

        public bool ExecutionCompleted { get; private set; }
        public long TimeStarted { get; private set; } = -1;
        public long TimeFinished { get; private set; } = -1;
    }

    class DummyExchangeCommand : ICommandMessageExchange<Guid>
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    class DummyExchangeHandler : ICommandHandler<DummyExchangeCommand, Guid>
    {
        public Task<Guid> ProcessAsync(DummyExchangeCommand command)
        {
            ExecutionCompleted = true;
            return Task.FromResult(Guid.NewGuid());
        }

        public bool ExecutionCompleted { get; private set; }
    }
}
