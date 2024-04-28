namespace Genius.Atom.Infrastructure.Threading;

public static class ExtensionMethods
{
    /// <summary>
    ///   Tries to enter the lock in read mode and returns a disposable object, which will free the locker up at the end.
    /// </summary>
    public static IDisposable BeginReadLock(this ReaderWriterLockSlim readerWriterLockSlim)
    {
        readerWriterLockSlim.EnterReadLock();
        return new DisposableAction(readerWriterLockSlim.ExitReadLock);
    }

    /// <summary>
    ///   Tries to enter the lock in write mode and returns a disposable object, which will free the locker up at the end.
    /// </summary>
    public static IDisposable BeginWriteLock(this ReaderWriterLockSlim readerWriterLockSlim)
    {
        readerWriterLockSlim.EnterWriteLock();
        return new DisposableAction(readerWriterLockSlim.ExitWriteLock);
    }
}
