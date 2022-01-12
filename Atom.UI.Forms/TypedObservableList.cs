using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Genius.Atom.UI.Forms;

public interface ITypedObservableList : INotifyCollectionChanged
{
    Type ItemType { get; }
}

public sealed class TypedObservableList<TContract, TType> : ObservableCollection<TContract>, ITypedObservableList, ITypedList
{
    public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
    {
        return TypeDescriptor.GetProperties(typeof(TType));
    }

    public string GetListName(PropertyDescriptor[] listAccessors)
    {
        return string.Empty;
    }

    public Type ItemType => typeof(TType);
}
