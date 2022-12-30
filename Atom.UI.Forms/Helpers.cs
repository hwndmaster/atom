using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal static partial class Helpers
{
    private static readonly Regex _captionFromPropertyNameRegex = CaptionFromPropertyNameRegex();

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
        return _captionFromPropertyNameRegex.Replace(propertyName, " $1");
    }

    [GeneratedRegex("(?<=[^$])([A-Z])")]
    private static partial Regex CaptionFromPropertyNameRegex();
}
