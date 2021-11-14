using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal static class Helpers
{
    public static Type GetListItemType(object value)
    {
        if (value is ListCollectionView listCollectionView)
            value = listCollectionView.SourceCollection;

        if (value is ITypedObservableList typedObservableList)
            return typedObservableList.ItemType;

        return value.GetType().GetGenericArguments().Single();
    }

    public static string MakeCaptionFromPropertyName(string propertyName)
    {
        return Regex.Replace(propertyName, "(?<=[^$])([A-Z])", " $1");
    }
}
