/*
 * References:
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Run.cs
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Bold.cs
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Italic.cs
 *   https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Underline.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public sealed class TextInlineRichBlock : InlineRichBlock
{
    public TextInlineRichBlock(string content)
    {
        Content = content;
    }

    public string Content { get; }
    public bool Bold { get; init; }
    public bool Italic { get; init; }
    public bool Underline { get; init; }
}
