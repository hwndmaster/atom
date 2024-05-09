using Microsoft.VisualStudio.Threading;

namespace Genius.Atom.Infrastructure.Threading;

public sealed class JoinableTaskHelper : IDisposable
{
    private readonly JoinableTaskContext _joinableTaskContext;

    public JoinableTaskHelper()
    {
        _joinableTaskContext = new JoinableTaskContext();
        Factory = new JoinableTaskFactory(_joinableTaskContext);
    }

    public void Dispose()
    {
        _joinableTaskContext.Dispose();
    }

    public JoinableTaskFactory Factory { get; }
}
