namespace Genius.Atom.Data;

/// <summary>
/// Thrown when an optimistic-concurrency check fails while updating an entity: the
/// <see cref="ITimeStamped.LastModified"/> token supplied by the caller no longer matches the value
/// currently stored (the entity was changed elsewhere, or the same update was submitted twice).
/// </summary>
/// <remarks>
/// Derives from <see cref="InvalidOperationException"/> for backward compatibility with existing
/// exception handling that treats repository failures as invalid operations; callers that need the
/// structured details (entity name, id, versions) can catch this specific type instead.
/// </remarks>
public sealed class EntityVersionConflictException : InvalidOperationException
{
    public EntityVersionConflictException(
        string entityName,
        object id,
        DateTimeOffset storedLastModified,
        DateTimeOffset attemptedLastModified)
        : base($"Entity '{entityName}' with ID {id} has a version conflict. "
            + $"Stored version: {storedLastModified:O}, attempted version: {attemptedLastModified:O}.")
    {
        EntityName = entityName;
        Id = id;
        StoredLastModified = storedLastModified;
        AttemptedLastModified = attemptedLastModified;
    }

    /// <summary>The name of the conflicting entity type (e.g. "Record").</summary>
    public string EntityName { get; }

    /// <summary>The identifier of the conflicting entity.</summary>
    public object Id { get; }

    /// <summary>The <see cref="ITimeStamped.LastModified"/> value currently stored.</summary>
    public DateTimeOffset StoredLastModified { get; }

    /// <summary>The <see cref="ITimeStamped.LastModified"/> value supplied by the caller.</summary>
    public DateTimeOffset AttemptedLastModified { get; }
}
