namespace Genius.Atom.Infrastructure.TestingUtil.Io;

internal sealed class MemoryStreamWrapper : MemoryStream
{
    private readonly Action<byte[]> _whenDisposedHandler;

    internal MemoryStreamWrapper(Action<byte[]> whenDisposedHandler)
    {
        _whenDisposedHandler = whenDisposedHandler;
    }

    protected override void Dispose(bool disposing)
    {
        Seek(0, SeekOrigin.Begin);
        var buffer = new byte[Length];
        Read(buffer, 0, buffer.Length);
        _whenDisposedHandler(buffer);

        base.Dispose(disposing);
    }
}
