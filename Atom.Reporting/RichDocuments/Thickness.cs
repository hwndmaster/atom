/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Thickness.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

public readonly struct Thickness
{
    public Thickness(double thickness)
    {
        Left = thickness;
        Top = thickness;
        Right = thickness;
        Bottom = thickness;
    }

    public Thickness(double left, double top, double right, double bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public double Bottom { get; }
    public double Left { get; }
    public double Right { get; }
    public double Top { get; }
}
