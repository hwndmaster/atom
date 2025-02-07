using System.Windows.Threading;

namespace Genius.Atom.UI.Forms.TestingUtil;

public static class UiFormsTestHelper
{
    public static void SetupDispatcher()
    {
        var frame = new DispatcherFrame();
#pragma warning disable VSTHRD110 // Observe result of async calls
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
#pragma warning restore VSTHRD110 // Observe result of async calls
        Dispatcher.PushFrame(frame);
    }
}
