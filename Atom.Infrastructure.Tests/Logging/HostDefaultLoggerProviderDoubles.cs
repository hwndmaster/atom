// Stand-ins for the logger providers the .NET generic host registers by default.
//
// LoggingModule matches those providers by full type name, because Atom.Infrastructure references only
// Microsoft.Extensions.Logging.Abstractions and should not drag in Microsoft.Extensions.Logging.Console
// and friends. These doubles therefore live in the same namespaces under the same names, which is what
// makes them exercise the real matching rule rather than a paraphrase of it.
//
// Nothing here collides with the genuine types: the test project does not reference those packages, and
// if it ever does, the ambiguity will surface as a compile error rather than a silently passing test.

namespace Microsoft.Extensions.Logging.Console
{
    internal sealed class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => throw new NotSupportedException();
        public void Dispose() { }
    }
}

namespace Microsoft.Extensions.Logging.Debug
{
    internal sealed class DebugLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => throw new NotSupportedException();
        public void Dispose() { }
    }
}
