using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Genius.Atom.UI.Forms;

internal static partial class Helpers
{
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
        return CaptionFromPropertyNameRegex().Replace(propertyName, " $1");
    }

    // TODO: Temporarily having error CS8795 after upgrading to .NET9:
    // "Partial method '...' must have an implementation part because it has accessibility modifiers."
    // Note: Same issues described here but not resolved:
    // - https://github.com/dotnet/roslyn/issues/69522
    // - https://github.com/dotnet/roslyn/issues/73964
    //[GeneratedRegex("(?<=[^$])([A-Z])")]
    //private static partial Regex CaptionFromPropertyNameRegex();
    private static Regex _captionFromPropertyNameRegex = new Regex("(?<=[^$])([A-Z])", RegexOptions.Compiled);
    private static Regex CaptionFromPropertyNameRegex() => _captionFromPropertyNameRegex;
}
