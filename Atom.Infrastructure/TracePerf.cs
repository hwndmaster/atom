using System.Diagnostics;

namespace Genius.Atom.Infrastructure;

public sealed class TracePerf
{
    private readonly Stopwatch _sw = new();
    private readonly string _name;
    private readonly string _message;

    private TracePerf(string name, string message)
    {
        _name = name;
        _message = message;
    }

    public static TracePerf Start<T>(string message)
    {
        return new TracePerf(typeof(T).Name, message).Start();
    }

    public TracePerf Start()
    {
        _sw.Restart();
        return this;
    }

    public void StopAndReport()
    {
        _sw.Stop();
        Trace.WriteLine($"[TracePerf] {_name} - {_message} took {_sw.ElapsedMilliseconds} ms");
    }
}
