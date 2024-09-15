using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.TestingUtil;

public record FakeLogRecord(LogLevel LogLevel, string Message, Exception? Exception);

public class FakeLogger : ILogger
{
    private readonly List<FakeLogRecord> _logs = new();

    public FakeLogger()
    {
        Logs = new ReadOnlyCollection<FakeLogRecord>(_logs);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Guard.NotNull(formatter);

        _logs.Add(new FakeLogRecord(logLevel, formatter(state, exception), exception));
    }

    public IReadOnlyCollection<FakeLogRecord> Logs { get; }
}

public sealed class FakeLogger<T> : FakeLogger, ILogger<T>
{
}
