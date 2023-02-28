/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/TextElement.cs
 */

using System.Drawing;

namespace Genius.Atom.Reporting.RichDocuments;

public abstract class RichBlockBase
{
    public Color? BackgroundColor { get; init; }
    public string? FontFamily { get; init; }
    public double? FontSize { get; init; }
    public FontStretch? FontStretch { get; init; }
    public Color? ForegroundColor { get; init; }
}
