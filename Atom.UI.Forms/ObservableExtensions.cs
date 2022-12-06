using System.Collections.Specialized;
using System.Reactive.Linq;

namespace Genius.Atom.UI.Forms;

public static class ObservableExtensions
{
    public static IObservable<NotifyCollectionChangedEventArgs> WhenCollectionChanged(this INotifyCollectionChanged collection)
    {
        return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            h => collection.CollectionChanged += h, h => collection.CollectionChanged -= h)
            .Select(x => x.EventArgs);
    }
}
