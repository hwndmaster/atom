using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal static class Helpers
{
    private static readonly Regex CaptionFromPropertyNameRegex = new(
        "(?<=[^$])([A-Z])", RegexOptions.Compiled, TimeSpan.FromSeconds(10));

    public static Type GetListItemType(object value)
    {
        if (value is CollectionViewSource collectionViewSource)
            value = collectionViewSource.View.SourceCollection;

        if (value is ListCollectionView listCollectionView)
            value = listCollectionView.SourceCollection;

        if (value is ITypedObservableCollection typedObservableCollection)
            return typedObservableCollection.ItemType;

        return value.GetType().GetGenericArguments().Single();
    }

    public static string MakeCaptionFromPropertyName(string propertyName)
    {
        return CaptionFromPropertyNameRegex.Replace(propertyName, " $1");
    }
}
