using System.Collections.Immutable;
using Genius.Atom.Infrastructure.Commands;

namespace Genius.Atom.Infrastructure.TestingUtil.Commands;

public sealed class FakeCommandBus : ICommandBus
{
    private readonly IFixture _fixture = InfrastructureTestHelper.CreateFixture();
    private readonly List<ICommandMessage> _sentCommands = new();
    private readonly List<object?> _receivedResults = new();

    public void AssertNoCommandOfType<T>()
        where T : ICommandMessage
    {
        Assert.False(_sentCommands.OfType<T>().Any());
    }

    public void AssertNoCommandsButOfType<T>()
        where T : ICommandMessage
    {
        Assert.False(_sentCommands.Any(x => x is not T));
    }

    public void AssertSingleCommand<T>(Func<T, bool> condition)
        where T : ICommandMessage
    {
        Assert.NotNull(condition);
        var command = GetSingleCommand<T>();
        Assert.True(condition(command));
    }

    public void AssertSingleCommand<T>(params Action<T>[] actionToAssert)
        where T : ICommandMessage
    {
        Assert.NotNull(actionToAssert);
        var command = GetSingleCommand<T>();
        foreach (var action in actionToAssert)
        {
            action(command);
        }
    }

    public T GetSingleCommand<T>()
        where T : ICommandMessage
    {
        var matchedEvents = _sentCommands.OfType<T>().ToList();
        Assert.Single(matchedEvents);
        return matchedEvents[0];
    }

    public Task SendAsync(ICommandMessage command)
    {
        _sentCommands.Add(command);
        return Task.CompletedTask;
    }

    public Task<TResult> SendAsync<TResult>(ICommandMessageExchange<TResult> command)
    {
        _sentCommands.Add(command);
        var result = _fixture.Create<TResult>();
        _receivedResults.Add(result);
        return Task.FromResult(result);
    }

    public ImmutableArray<object?> ReceivedResults => _receivedResults.ToImmutableArray();
    public ImmutableArray<ICommandMessage> SentCommands => _sentCommands.ToImmutableArray();
}
