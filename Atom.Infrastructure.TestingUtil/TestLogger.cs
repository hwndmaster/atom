using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.TestingUtil;

public record TestLogRecord(LogLevel LogLevel, string Message, Exception? Exception);

public class TestLogger : ILogger
{
    private readonly List<TestLogRecord> _logs = new();

    public TestLogger()
    {
        Logs = new ReadOnlyCollection<TestLogRecord>(_logs);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Guard.NotNull(formatter);

        _logs.Add(new TestLogRecord(logLevel, formatter(state, exception), exception));
    }

    public IReadOnlyCollection<TestLogRecord> Logs { get; }
}

public sealed class TestLogger<T> : TestLogger, ILogger<T>
{
}
