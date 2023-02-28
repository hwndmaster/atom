using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;
using Genius.Atom.UI.Forms;

namespace Genius.Atom.Reporting.UI.RichDocuments;

internal sealed class HyperlinkTextRichBlockConverter : RichBlockBaseConverter, IInlineRichBlockConverter
{
    private readonly Lazy<IEnumerable<IInlineRichBlockConverter>> _inlineConverters;

    public HyperlinkTextRichBlockConverter(Lazy<IEnumerable<IInlineRichBlockConverter>> inlineConverters)
    {
        _inlineConverters = inlineConverters.NotNull();
    }

    public bool CanConvert(InlineRichBlock block)
        => block is HyperlinkInlineRichBlock;

    public Inline Convert(InlineRichBlock block)
    {
        var hyperlinkBlock = (HyperlinkInlineRichBlock)block;
        var resultingHyperlink = new Hyperlink();
        ConvertBlockBaseProperties(block, resultingHyperlink);
        var action = hyperlinkBlock.Action;
        resultingHyperlink.Command = new ActionCommand(_ => action());
        foreach (var inline in hyperlinkBlock.Inlines)
        {
            var resultingInline = _inlineConverters.Value.First(x => x.CanConvert(inline)).Convert(inline);
            resultingHyperlink.Inlines.Add(resultingInline);
        }

        return resultingHyperlink;
    }
}
