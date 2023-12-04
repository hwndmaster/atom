using System.Collections.Specialized;
using System.ComponentModel;

namespace Genius.Atom.UI.Forms;

public interface ITypedObservableCollection : INotifyCollectionChanged
{
    Type ItemType { get; }
}

public sealed class TypedObservableCollection<TContract, TType> : DelayedObservableCollection<TContract>, ITypedObservableCollection, ITypedList
{
    public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[]? listAccessors)
    {
        return TypeDescriptor.GetProperties(typeof(TType));
    }

    public string GetListName(PropertyDescriptor[]? listAccessors)
    {
        return string.Empty;
    }

    public Type ItemType => typeof(TType);
}
