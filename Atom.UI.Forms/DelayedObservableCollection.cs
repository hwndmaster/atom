using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

// NOTE: The delay and suspension notifications functionality has originally been taken from:
//       https://github.com/ENikS/DelayedObservableCollection/blob/master/DelayedObservableCollection/DelayedObservableCollection.cs

public class DelayedObservableCollection<T> : Collection<T>,
    INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
    private const string _countString = "Count";

    /// <summary>
    ///   This must agree with Binding.IndexerName. It is declared separately
    ///   here so as to avoid a dependency on PresentationFramework.dll.
    /// </summary>
    private const string _indexerName = "Item[]";

    private readonly ReentryMonitor _monitor = new();
    private readonly NotificationInfo? _notifyInfo;

    /// <summary>
    /// Indicates if modification of container allowed during change notification.
    /// </summary>
    private bool _disableReentry;

    private Action FireCountAndIndexerChanged = delegate { };
    private Action FireIndexerChanged = delegate { };

    /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged" />
    protected event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged" />
    protected event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Initializes a new instance of DelayedObservableCollection that is empty and has default initial capacity.
    /// </summary>
    public DelayedObservableCollection()
        : base()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the DelayedObservableCollection class
    ///   that contains elements copied from the specified list
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new list.</param>
    /// <remarks>
    ///   The elements are copied onto the DelayedObservableCollection in the
    ///   same order they are read by the enumerator of the list.
    /// </remarks>
    /// <exception cref="ArgumentNullException">When <paramref name="list"/> is a null reference.</exception>
    public DelayedObservableCollection(List<T> list)
        : base(list)
    {
        foreach (T item in list)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    ///   Initializes a new instance of the DelayedObservableCollection class that contains
    ///   elements copied from the specified collection and has sufficient capacity
    ///   to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <remarks>
    ///   The elements are copied onto the ObservableCollection in the
    ///   same order they are read by the enumerator of the collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException">When <paramref name="collection"/> is a null reference.</exception>
    public DelayedObservableCollection(IEnumerable<T> collection)
    {
        Guard.NotNull(collection);

        foreach (var item in collection)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// Constructor that configures the container to delay or disable notifications.
    /// </summary>
    /// <param name="parent">Reference to an original collection whose events are being postponed</param>
    /// <param name="notify">Specifies if notifications needs to be delayed or disabled</param>
    public DelayedObservableCollection(DelayedObservableCollection<T> parent, bool notify)
        : base(parent.Items)
    {
        _notifyInfo = new NotificationInfo
        {
            RootCollection = parent
        };

        if (notify)
        {
            CollectionChanged = _notifyInfo.Initialize();
        }
    }

    /// <summary>
    ///   A destructor
    /// </summary>
    ~DelayedObservableCollection()
    {
        Dispose(false);
    }

    /// <inheritdoc cref="INotifyPropertyChanged.PropertyChanged" />
    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            if (_notifyInfo is null)
            {
                if (PropertyChanged is null)
                {
                    FireCountAndIndexerChanged = delegate
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(_countString));
                        OnPropertyChanged(new PropertyChangedEventArgs(_indexerName));
                    };
                    FireIndexerChanged = delegate
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(_indexerName));
                    };
                }

                PropertyChanged += value;
            }
            else
            {
                _notifyInfo.RootCollection.PropertyChanged += value;
            }
        }

        remove
        {
            if (_notifyInfo is null)
            {
                PropertyChanged -= value;

                if (PropertyChanged is null)
                {
                    FireCountAndIndexerChanged = delegate { };
                    FireIndexerChanged = delegate { };
                }
            }
            else
            {
                _notifyInfo.RootCollection.PropertyChanged -= value;
            }
        }
    }

    /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged" />
    event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add
        {
            if (_notifyInfo is null)
            {
                if (value is null)
                    return;

                CollectionChanged += value;
                _disableReentry = CollectionChanged.GetInvocationList().Length > 1;
            }
            else
            {
                _notifyInfo.RootCollection.CollectionChanged += value;
            }
        }

        remove
        {
            if (_notifyInfo is null)
            {
                CollectionChanged -= value;

                _disableReentry = CollectionChanged?.GetInvocationList().Length > 1;
            }
            else
            {
                _notifyInfo.RootCollection.CollectionChanged -= value;
            }
        }
    }

    /// <summary>
    ///   Moves an item at <paramref name="oldIndex"/> to <paramref name="newIndex"/>.
    /// </summary>
    public void Move(int oldIndex, int newIndex)
    {
        CheckReentrancy();

        T removedItem = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, removedItem);

        FireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex));
    }

    /// <summary>
    ///   Returns an instance of <see cref="DelayedObservableCollection"/>
    ///   class which manipulates original collection but suppresses notifications
    ///   until this instance has been released and Dispose() method has been called.
    ///   To suppress notifications it is recommended to use this instance inside
    ///   using() statement:
    ///   <code>
    ///         using (var iSuppressed = collection.DelayNotifications())
    ///         {
    ///             iSuppressed.Add(x);
    ///             iSuppressed.Add(y);
    ///             iSuppressed.Add(z);
    ///         }
    ///   </code>
    ///   Each delayed interface is bound to only one type of operation such as Add, Remove, etc.
    ///   Different types of operation on the same delayed interface are not allowed. In order to
    ///   do other type of operation you can allocate another wrapper by calling .DelayNotifications() on
    ///   either original object or any delayed instances.
    /// </summary>
    public DelayedObservableCollection<T> DelayNotifications()
    {
        return new DelayedObservableCollection<T>((_notifyInfo is null) ? this : _notifyInfo.RootCollection, true);
    }

    /// <summary>
    ///   Returns a wrapper instance of an DelayedObservableCollection class.
    ///   Calling methods of this instance will modify original collection
    ///   but will not generate any notifications.
    /// </summary>
    public DelayedObservableCollection<T> DisableNotifications()
    {
        return new DelayedObservableCollection<T>((_notifyInfo is null) ? this : _notifyInfo.RootCollection, false);
    }

    public void ReplaceItems(IEnumerable<T> items)
    {
        Clear();

        using var delayed = DelayNotifications();
        foreach (var item in items)
        {
            delayed.Add(item);
        }
    }

    /// <summary>
    ///   Called by base class Collection&lt;T&gt; when the list is being cleared;
    ///   raises a CollectionChanged event to any listeners.
    /// </summary>
    protected override void ClearItems()
    {
        CheckReentrancy();

        base.ClearItems();

        FireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///   Called by base class Collection&lt;T&gt; when an item is removed from list;
    ///   raises a CollectionChanged event to any listeners.
    /// </summary>
    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        T removedItem = this[index];

        base.RemoveItem(index);

        FireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
    }

    /// <summary>
    ///   Called by base class Collection&lt;T&gt; when an item is added to list;
    ///   raises a CollectionChanged event to any listeners.
    /// </summary>
    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();

        base.InsertItem(index, item);

        FireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    /// <summary>
    ///   Called by base class Collection&lt;T&gt; when an item is set in list;
    ///   raises a CollectionChanged event to any listeners.
    /// </summary>
    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();

        T originalItem = this[index];
        base.SetItem(index, item);

        FireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem, item, index));
    }

    /// <summary>
    ///   Raises a <see cref="PropertyChanged"/> event.
    /// </summary>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (PropertyChanged is not null)
        {
            PropertyChanged(this, e);
        }
    }

    /// <summary>
    ///   Raises <see cref="CollectionChanged"/> event to the listeners.
    ///   Properties/methods modifying this <see cref="DelayedObservableCollection{T}"/> are raising
    ///   a collection changed event using this virtual method.
    /// </summary>
    /// <remarks>
    ///   When overriding this method, either call its base implementation
    ///   or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (CollectionChanged is null)
            return;

        using (BlockReentrancy())
        {
            CollectionChanged(this, e);
        }
    }

    /// <summary>
    ///   Disallow reentrant attempts to change this collection. E.g. an event handler
    ///   of the <see cref="CollectionChanged"/> event is not allowed to make changes to this collection.
    /// </summary>
    /// <remarks>
    ///   typical usage is to wrap e.g. a <see cref="OnCollectionChanged"/> call with a using() scope:
    ///   <code>
    ///         using (BlockReentrancy())
    ///         {
    ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
    ///         }
    ///   </code>
    /// </remarks>
    protected IDisposable BlockReentrancy()
    {
        return _monitor.Enter();
    }

    /// <summary>
    ///   Check and assert for reentrant attempts to change this collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when changing the collection
    ///   while another collection change is still being notified to other listeners.
    /// </exception>
    protected void CheckReentrancy()
    {
        // Can only allow changes if there's only one listener - the problem
        // only arises if reentrant changes make the original event args
        // invalid for later listeners.  This keeps existing code working
        // (e.g. Selector.SelectedItems).
        if (_disableReentry && _monitor.IsNotifying)
        {
            throw new InvalidOperationException("DelayedObservableCollection Reentrancy Not Allowed");
        }
    }

    /// <summary>
    ///   Called by the application code to fire all delayed notifications.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Fires notification with all accumulated events
    /// </summary>
    /// <param name="reason">True is called by App code. False if called from GC.</param>
    protected virtual void Dispose(bool reason)
    {
        // Fire delayed notifications
        if (_notifyInfo is not null)
        {
            if (_notifyInfo.HasEventArgs)
            {
                if (_notifyInfo.RootCollection.PropertyChanged is not null)
                {
                    if (_notifyInfo.IsCountChanged)
                        _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(_countString));

                    _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(_indexerName));
                }

                using (_notifyInfo.RootCollection.BlockReentrancy())
                {
                    NotifyCollectionChangedEventArgs args = _notifyInfo.EventArgs;

                    if (_notifyInfo.RootCollection.CollectionChanged is not null)
                    {
                        foreach (Delegate delegateItem in _notifyInfo.RootCollection.CollectionChanged.GetInvocationList())
                        {
                            if (delegateItem.Target is ListCollectionView
                                && (
                                    (args.Action == NotifyCollectionChangedAction.Add && args.NewItems?.Count > 1)
                                    || (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems?.Count > 1)))
                            {
                                // To prevent "Range actions are not supported." NotSupportedException
                                // in ListCollectionView we change the eventual event to Action == Reset
                                // Ref source: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Data/ListCollectionView.cs,2532
                                var argsForLcv = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                                delegateItem.DynamicInvoke(new object[] { _notifyInfo.RootCollection, argsForLcv });
                                continue;
                            }
                            delegateItem.DynamicInvoke(new object[] { _notifyInfo.RootCollection, args });
                        }
                    }
                }

                // Reset and reuse if necessary
                CollectionChanged = _notifyInfo.Initialize();
            }
        }
    }

    private class ReentryMonitor : IDisposable
    {
        private int _referenceCount;

        public IDisposable Enter()
        {
            ++_referenceCount;

            return this;
        }

        public void Dispose()
            => --_referenceCount;

        public bool IsNotifying => _referenceCount != 0;
    }

    private class NotificationInfo
    {
        private NotifyCollectionChangedAction? _action;
        private IList _newItems = Array.Empty<object>();
        private IList _oldItems = Array.Empty<object>();
        private int _newIndex;
        private int _oldIndex;

        public NotifyCollectionChangedEventHandler Initialize()
        {
            _action = null;
            _newItems = Array.Empty<object>();
            _oldItems = Array.Empty<object>();

            return (sender, args) =>
            {
                DelayedObservableCollection<T>? wrapper = sender as DelayedObservableCollection<T>;
                Debug.Assert(wrapper is not null, "Calling object must be DelayedObservableCollection<T>");
                Debug.Assert(wrapper._notifyInfo is not null, "Calling object must be Delayed wrapper.");

                // Setup
                _action = args.Action;

                switch (_action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _newItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            if (e.NewItems is not null)
                            {
                                foreach (T item in e.NewItems)
                                    _newItems.Add(item);
                            }
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        _oldItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            if (e.OldItems is not null)
                            {
                                foreach (T item in e.OldItems)
                                    _oldItems.Add(item);
                            }
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        _newItems = new List<T>();
                        _oldItems = new List<T>();
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            if (e.NewItems is not null)
                            {
                                foreach (T item in e.NewItems)
                                    _newItems.Add(item);
                            }

                            if (e.OldItems is not null)
                            {
                                foreach (T item in e.OldItems)
                                    _oldItems.Add(item);
                            }
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        _newIndex = args.NewStartingIndex;
                        _newItems = args.NewItems ?? Array.Empty<object>();
                        _oldIndex = args.OldStartingIndex;
                        _oldItems = args.OldItems ?? Array.Empty<object>();
                        wrapper.CollectionChanged = (s, e)
                            => throw new InvalidOperationException($"Due to design of {nameof(NotifyCollectionChangedEventArgs)} combination of multiple Move operations is not possible");
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) => AssertActionType(e);
                        break;
                }
            };
        }

        private void AssertActionType(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != _action)
            {
                throw new InvalidOperationException(
                    string.Format("Attempting to perform {0} during {1}. Mixed actions on the same delayed interface are not allowed.",
                    e.Action, _action));
            }
        }

        public required DelayedObservableCollection<T> RootCollection { get; init; }

        public bool IsCountChanged { get; private set; }

        public NotifyCollectionChangedEventArgs EventArgs
        {
            get
            {
                return _action switch
                {
                    NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(_action.Value),
                    NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(_action.Value, _newItems),
                    NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(_action.Value, _oldItems),
                    NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(_action.Value, _oldItems?[0], _newIndex, _oldIndex),
                    NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(_action.Value, _newItems, _oldItems),
                    _ => throw new NotSupportedException($"The {_action} action is not supported."),
                };
            }
        }

        public bool HasEventArgs => _action.HasValue;
    }
}
