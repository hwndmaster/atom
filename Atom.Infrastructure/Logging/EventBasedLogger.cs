using Genius.Atom.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Logging;

internal sealed class EventBasedLogger : ILogger
{
    private readonly IEventBus _eventBus;
    private readonly string _shortName;

    public EventBasedLogger(string name, IEventBus eventBus)
    {
        _shortName = CreateShortNameFrom(name);
        _eventBus = eventBus;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
        // Ignore all log events below Information
        => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        _eventBus.Publish(new LogEvent(logLevel, _shortName, message));
    }

    private static string CreateShortNameFrom(string name)
    {
        if (name.StartsWith("Genius."))
        {
            return string.Join('.', name.Split('.').Skip(3));
        }

        return name;
    }
}
