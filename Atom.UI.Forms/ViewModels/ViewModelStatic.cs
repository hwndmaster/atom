using Genius.Atom.Infrastructure.Threading;
using Microsoft.VisualStudio.Threading;

namespace Genius.Atom.UI.Forms;

/// <summary>
///   Static helpers for <see cref="ViewModelBase"/> for internal use.
/// </summary>
internal static class ViewModelStatic
{
    private static readonly JoinableTaskHelper _joinableTask = new();

    internal static JoinableTaskFactory JoinableTaskFactory
    {
        get => _joinableTask.Factory;
    }
}
