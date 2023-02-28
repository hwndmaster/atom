using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

internal sealed class ListRichBlockConverter : RichBlockConverter
{
    private readonly Lazy<IEnumerable<IRichBlockConverter>> _converters;

    public ListRichBlockConverter(Lazy<IEnumerable<IRichBlockConverter>> converters)
    {
        _converters = converters.NotNull();
    }

    public override bool CanConvert(RichBlock block)
        => block is ListRichBlock;

    public override Block Convert(RichBlock block)
    {
        var list = (ListRichBlock)block;
        var resultingList = new List();
        ConvertBlockBaseProperties(block, resultingList);
        foreach (var item in list.ListItems)
        {
            var listItem = new ListItem();
            listItem.Blocks.Add(_converters.Value.First(x => x.CanConvert(item)).Convert(item));
            resultingList.ListItems.Add(listItem);
        }
        return resultingList;
    }
}
