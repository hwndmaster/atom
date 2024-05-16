namespace Genius.Atom.Infrastructure.Tests;

#pragma warning disable IDISP017 // Prefer using

public sealed class DisposerTests
{
    [Fact]
    public void Dispose_SwitchesIsDisposed()
    {
        // Arrange
        var disposer = new Disposer();

        // Act
        disposer.Dispose();

        // Verify
        Assert.True(disposer.IsDisposed);
    }

    [Fact]
    public void DisposeAction()
    {
        // Arrange
        var disposed = false;
        var disposer = new Disposer();

        // Act
        disposer.Add(() => disposed = true);
        disposer.Dispose();

        // Verify
        Assert.True(disposed);
    }

    [Fact]
    public void DisposeObject()
    {
        // Arrange
        var disposableObject = new DisposableObject();
        var disposer = new Disposer();

        // Pre-verify
        Assert.False(disposableObject.IsDisposed);

        // Act
        disposer.Add(disposableObject);
        disposer.Dispose();

        // Verify
        Assert.True(disposableObject.IsDisposed);
    }

    [Fact]
    public void Dispose_InReversedOrder()
    {
        // Arrange
        var disposedOrder = new List<int>();
        var disposer = new Disposer();

        // Act
        disposer.Add(() => disposedOrder.Add(1));
        disposer.Add(() => disposedOrder.Add(2));
        disposer.Dispose();

        // Verify
        Assert.Equal([2, 1], disposedOrder);
    }

    class DisposableObject : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
