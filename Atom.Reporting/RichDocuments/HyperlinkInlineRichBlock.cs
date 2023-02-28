/*
 * Reference:
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Hyperlink.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class HyperlinkInlineRichBlock : InlineRichBlock
{
    public HyperlinkInlineRichBlock(Action action, params InlineRichBlock[] inlines)
    {
        Action = action.NotNull();
        Inlines.AddRange(inlines);
    }

    public Action Action { get; }
    public List<InlineRichBlock> Inlines { get; } = new();
}
