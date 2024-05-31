using System.Collections.Specialized;
using System.Reactive.Linq;
using Genius.Atom.Infrastructure.Tasks;
using Genius.Atom.Infrastructure.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.UI.Forms;

public static class ObservableExtensions
{
    /// <summary>
    ///   Subscribes an asynchronous element handler to an observable sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    public static IDisposable Subscribe<T>(this IObservable<T> source, Func<T, Task> onNext)
    {
        return source.Subscribe(value =>
        {
            var joinableTaskHelper = Module.ServiceProvider.GetRequiredService<JoinableTaskHelper>();
            joinableTaskHelper.Factory.Run(async () =>
            {
                await onNext(value);
                joinableTaskHelper.Dispose();
            });
        });
    }

    /// <summary>
    ///   Subscribes an element handler to an observable sequence to be performed on UI thread.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
    public static IDisposable SubscribeOnUiThread<T>(this IObservable<T> source, Action<T> onNext)
    {
        var dispatcher = Module.ServiceProvider.GetRequiredService<IUiDispatcher>();
        return SubscribeOnUiThread(source, dispatcher, onNext);
    }

    /// <summary>
    ///   Subscribes an element handler to an observable sequence to be performed on UI thread using the specified <paramref name="uiDispatcher"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="uiDispatcher">UI dispatcher to be used.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
    public static IDisposable SubscribeOnUiThread<T>(this IObservable<T> source, IUiDispatcher uiDispatcher, Action<T> onNext)
    {
        Guard.NotNull(source);
        Guard.NotNull(onNext);

        return source.Subscribe(value =>
        {
            uiDispatcher.Invoke(() => onNext(value));
        });
    }

    /// <summary>
    ///   Subscribes an asynchronous element handler to an observable sequence to be performed on UI thread.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">An asynchronous action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
    public static IDisposable SubscribeOnUiThread<T>(this IObservable<T> source, Func<T, Task> onNext)
    {
        var dispatcher = Module.ServiceProvider.GetRequiredService<IUiDispatcher>();
        return SubscribeOnUiThread(source, dispatcher, onNext);
    }

    /// <summary>
    ///   Subscribes an asynchronous element handler to an observable sequence to be performed on UI thread using the specified <paramref name="uiDispatcher"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="uiDispatcher">UI dispatcher to be used.</param>
    /// <param name="onNext">An asynchronous action to invoke for each element in the observable sequence.</param>
    /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
    public static IDisposable SubscribeOnUiThread<T>(this IObservable<T> source, IUiDispatcher uiDispatcher, Func<T, Task> onNext)
    {
        Guard.NotNull(source);
        Guard.NotNull(onNext);

        return source.Subscribe(value =>
        {
            uiDispatcher.InvokeAsync(async () => await onNext(value)).RunAndForget();
        });
    }

    /// <summary>
    ///   Subscribes an element handler to an observable sequence without necessity to dispose the handler.
    ///   Usually used on Reactive subscriptions, running in View Models over changes in local properties.
    ///   The main purpose of this method is to satisfy
    ///   CA2000 (https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2000)
    ///   analysis rule.
    ///   NOTE: Never use it on any other Reactive subscription, running on not-owned properties.
    /// </summary>
    /// <param name="source">Observable sequence to subscribe to.</param>
    /// <param name="onNext">Action to invoke for each element in the observable sequence.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="onNext"/> is <c>null</c>.</exception>
    public static void SubscribeNoDisposal<T>(this IObservable<T> source, Action<T> onNext)
    {
        Guard.NotNull(source);
        Guard.NotNull(onNext);

        source.Subscribe(value => onNext(value));
    }

    public static IObservable<NotifyCollectionChangedEventArgs> WhenCollectionChanged(this INotifyCollectionChanged collection)
    {
        return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            h => collection.CollectionChanged += h, h => collection.CollectionChanged -= h)
            .Select(x => x.EventArgs);
    }
}
