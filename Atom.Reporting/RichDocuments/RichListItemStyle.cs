/*
 * Reference: https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/System/Windows/TextAndFontProperties.cs
 *            the TextMarkerStyle enum.
 */

namespace Genius.Atom.Reporting.RichDocuments;

public enum RichListItemStyle
{
    None = 0,
    Disc = 1,
    Circle = 2,
    Square = 3,
    Box = 4,
    LowerRoman = 5,
    UpperRoman = 6,
    LowerLatin = 7,
    UpperLatin = 8,
    Decimal = 9
}
