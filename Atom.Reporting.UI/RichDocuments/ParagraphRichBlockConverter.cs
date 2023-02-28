using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

internal sealed class ParagraphRichBlockConverter : RichBlockConverter
{
    private readonly IEnumerable<IInlineRichBlockConverter> _converters;

    public ParagraphRichBlockConverter(IEnumerable<IInlineRichBlockConverter> converters)
    {
        _converters = converters.NotNull();
    }

    public override bool CanConvert(RichBlock block)
        => block is ParagraphRichBlock;

    public override Block Convert(RichBlock block)
    {
        var paragraph = (ParagraphRichBlock)block;
        var resultingParagraph = new Paragraph();
        ConvertBlockProperties(block, resultingParagraph);
        foreach (var inline in paragraph.Inlines)
        {
            var converter = _converters.First(x => x.CanConvert(inline));
            resultingParagraph.Inlines.Add(converter.Convert(inline));
        }
        return resultingParagraph;
    }
}
