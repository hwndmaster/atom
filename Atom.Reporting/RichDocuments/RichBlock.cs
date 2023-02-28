/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Documents/Block.cs
 */

using System.Drawing;

namespace Genius.Atom.Reporting.RichDocuments;

public abstract class RichBlock : RichBlockBase
{
    public Color? BorderColor { get; set; }
    public Thickness? BorderThickness { get; set; }
    public Thickness? Padding { get; set; }
    public Thickness? Margin { get; set; }
    public double? LineHeight { get; set; }
    public RichTextAlignment? TextAlignment { get; set; }
}
