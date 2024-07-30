namespace Genius.Atom.UI.Forms.Tests;

public sealed class UiDispatcherTests : IDisposable
{
    private readonly UiDispatcher _sut = new();

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Invoke_HappyFlowScenario()
    {
        // Arrange
        bool processed = false;

        // Act
        _sut.Invoke(() => processed = true);

        // Verify
        Assert.True(processed);
    }

    [Fact]
    public async Task InvokeAsync_GivenAction_HappyFlowScenario()
    {
        // Arrange
        bool processed = false;

        // Act
        await _sut.InvokeAsync(() =>
        {
            processed = true;
        });

        // Verify
        Assert.True(processed);
    }

    [Fact]
    public async Task InvokeAsync_GivenTask_HappyFlowScenario()
    {
        // Arrange
        bool processed = false;

        // Act
        await _sut.InvokeAsync(async () =>
        {
            await Task.Run(() => processed = true).ConfigureAwait(false);
        });

        // Verify
        Assert.True(processed);
    }

    [Fact]
    public async Task InvokeAsync_GivenTask_WhenExceptionOccurred_ThenHandled()
    {
        // Arrange
        var task = _sut.InvokeAsync(async () =>
        {
            await Task.Run(() => throw new InvalidOperationException()).ConfigureAwait(false);
        });

        // Act & Verify
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task.ConfigureAwait(false));
    }
}
