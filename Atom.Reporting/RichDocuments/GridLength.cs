/*
 * Reference: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/GridLength.cs
 */

namespace Genius.Atom.Reporting.RichDocuments;

/// <summary>
///   GridUnitType enum is used to indicate what kind of value the GridLength is holding.
/// </summary>
public enum GridUnitType
{
    /// <summary>
    ///   The value indicates that content should be calculated without constraints.
    /// </summary>
    Auto = 0,

    /// <summary>
    ///   The value is expressed as a pixel.
    /// </summary>
    Pixel,

    /// <summary>
    ///   The value is expressed as a weighted proportion of available space.
    /// </summary>
    Star,
}

/// <summary>
///   GridLength is the type used for various length-like properties in the system,
///   that explicitly support Star unit type. For example, "Width", "Height"
///   properties of ColumnDefinition and RowDefinition used by Grid.
/// </summary>
public readonly struct GridLength : IEquatable<GridLength>
{
    private readonly double _unitValue; // unit value storage

    /// <summary>
    ///   Constructor, initializes the GridLength as absolute value in pixels.
    /// </summary>
    /// <param name="pixels">Specifies the number of 'device-independent pixels' (96 pixels-per-inch).</param>
    public GridLength(double pixels)
        : this(pixels, GridUnitType.Pixel)
    {
    }

    /// <summary>
    ///   Constructor, initializes the GridLength and specifies what kind of value it will hold.
    /// </summary>
    /// <param name="value">Value to be stored by this GridLength instance.</param>
    /// <param name="type">Type of the value to be stored by this GridLength instance.</param>
    /// <remarks>
    ///   If the <c>type</c> parameter is <c>GridUnitType.Auto</c>,
    ///   then passed in value is ignored and replaced with <c>0</c>.
    /// </remarks>
    public GridLength(double value, GridUnitType type)
    {
        _unitValue = (type == GridUnitType.Auto) ? 0.0 : value;
        GridUnitType = type;
    }

    /// <summary>
    ///   Overloaded operator, compares 2 GridLength's.
    /// </summary>
    /// <param name="gl1">first GridLength to compare.</param>
    /// <param name="gl2">second GridLength to compare.</param>
    /// <returns>true if specified GridLengths have same value and unit type.</returns>
    public static bool operator ==(GridLength gl1, GridLength gl2)
    {
        return gl1.GridUnitType == gl2.GridUnitType
            && gl1.Value == gl2.Value;
    }

    /// <summary>
    ///   Overloaded operator, compares 2 GridLength's.
    /// </summary>
    /// <param name="gl1">first GridLength to compare.</param>
    /// <param name="gl2">second GridLength to compare.</param>
    /// <returns>true if specified GridLengths have either different value or unit type.</returns>
    public static bool operator !=(GridLength gl1, GridLength gl2)
    {
        return gl1.GridUnitType != gl2.GridUnitType
            || gl1.Value != gl2.Value;
    }

    /// <summary>
    ///   Compares this instance of GridLength with another object.
    /// </summary>
    /// <param name="oCompare">Reference to an object for comparison.</param>
    /// <returns><c>true</c>if this GridLength instance has the same value and unit type as oCompare.</returns>
    override public bool Equals(object? oCompare)
    {
        if (oCompare is GridLength l)
        {
            return this == l;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    ///   Compares this instance of GridLength with another instance.
    /// </summary>
    /// <param name="gridLength">Grid length instance to compare.</param>
    /// <returns><c>true</c>if this GridLength instance has the same value and unit type as gridLength.</returns>
    public bool Equals(GridLength gridLength)
    {
        return this == gridLength;
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode()
    {
        return (int)_unitValue + (int)GridUnitType;
    }

    /// <summary>
    ///   Returns <c>true</c> if this GridLength instance holds
    ///   an absolute (pixel) value.
    /// </summary>
    public bool IsAbsolute => GridUnitType == GridUnitType.Pixel;

    /// <summary>
    ///   Returns <c>true</c> if this GridLength instance is
    ///   automatic (not specified).
    /// </summary>
    public bool IsAuto => GridUnitType == GridUnitType.Auto;

    /// <summary>
    ///   Returns <c>true</c> if this GridLength instance holds weighted proportion of available space.
    /// </summary>
    public bool IsStar => GridUnitType == GridUnitType.Star;

    /// <summary>
    ///   Returns value part of this GridLength instance.
    /// </summary>
    public double Value => (GridUnitType == GridUnitType.Auto) ? 1.0 : _unitValue;

    /// <summary>
    ///   Returns unit type of this GridLength instance.
    /// </summary>
    public GridUnitType GridUnitType { get; }

    /// <summary>
    ///   Returns initialized Auto GridLength value.
    /// </summary>
    public static GridLength Auto { get; } = new GridLength(1.0, GridUnitType.Auto);
}
