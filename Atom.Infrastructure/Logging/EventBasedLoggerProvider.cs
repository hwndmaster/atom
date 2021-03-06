using System.Collections.Concurrent;
using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Logging;

public sealed class EventBasedLoggerProvider : ILoggerProvider
{
    private readonly IEventBus _eventBus;

    private readonly ConcurrentDictionary<string, EventBasedLogger> _loggers = new();

    public EventBasedLoggerProvider(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new EventBasedLogger(name, _eventBus));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
