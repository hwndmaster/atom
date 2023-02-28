/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/List.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class ListRichBlock : RichBlock
{
    public RichListItemStyle ItemStyle { get; set; }
    public IList<RichBlock> ListItems { get; } = new List<RichBlock>();
    public int StartIndex { get; set; }
}
