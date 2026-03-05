namespace Genius.Atom.Data;

/// <summary>
/// Interface defining the updatable object which contains timestamp information.
/// </summary>
public interface ITimeStamped
{
    /// <summary>
    /// Gets the date and time when the entity was last modified.
    /// </summary>
    DateTimeOffset LastModified { get; }
}
