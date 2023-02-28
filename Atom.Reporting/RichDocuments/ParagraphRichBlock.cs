/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Paragraph.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class ParagraphRichBlock : RichBlock
{
    public ParagraphRichBlock()
    {
    }

    public ParagraphRichBlock(params InlineRichBlock[] inlines)
    {
        Inlines.AddRange(inlines);
    }

    public List<InlineRichBlock> Inlines { get; } = new();
}
