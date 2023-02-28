/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/FlowDocument.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class RichDocument
{
    public RichDocument(params RichBlock[] blocks)
    {
        Blocks.AddRange(blocks);
    }

    public List<RichBlock> Blocks { get; } = new();
}
