using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

internal sealed class TextInlineRichBlockConverter : RichBlockBaseConverter, IInlineRichBlockConverter
{
    public bool CanConvert(InlineRichBlock block)
        => block is TextInlineRichBlock;

    public Inline Convert(InlineRichBlock block)
    {
        var textBlock = (TextInlineRichBlock)block;
        Inline resultingText = new Run(textBlock.Content);
        ConvertBlockBaseProperties(block, resultingText);
        if (textBlock.Bold)
        {
            resultingText = new Bold(resultingText);
        }
        if (textBlock.Italic)
        {
            resultingText = new Italic(resultingText);
        }
        if (textBlock.Underline)
        {
            resultingText = new Underline(resultingText);
        }

        return resultingText;
    }
}
