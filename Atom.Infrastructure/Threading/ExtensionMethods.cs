namespace Genius.Atom.Infrastructure.Threading;

public static class ExtensionMethods
{
    extension(ReaderWriterLockSlim readerWriterLockSlim)
    {
        /// <summary>
        ///   Tries to enter the lock in read mode and returns a disposable object, which will free the locker up at the end.
        /// </summary>
        public IDisposable BeginReadLock()
        {
            readerWriterLockSlim.EnterReadLock();
            return new DisposableAction(readerWriterLockSlim.ExitReadLock);
        }

        /// <summary>
        ///   Tries to enter the lock in write mode and returns a disposable object, which will free the locker up at the end.
        /// </summary>
        public IDisposable BeginWriteLock()
        {
            readerWriterLockSlim.EnterWriteLock();
            return new DisposableAction(readerWriterLockSlim.ExitWriteLock);
        }
    }
}
