using System.Windows.Documents;
using Genius.Atom.Reporting.RichDocuments;

namespace Genius.Atom.Reporting.UI.RichDocuments;

internal sealed class LineBreakInlineRichBlockConverter : IInlineRichBlockConverter
{
    public bool CanConvert(InlineRichBlock block)
        => block is LineBreakRichBlock;

    public Inline Convert(InlineRichBlock block)
    {
        return new LineBreak();
    }
}
