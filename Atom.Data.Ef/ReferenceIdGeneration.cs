namespace Genius.Atom.Data.Ef;

/// <summary>
/// Defines who assigns the value of a strongly-typed primary key when an entity is inserted.
/// </summary>
public enum ReferenceIdGeneration
{
    /// <summary>
    /// The database assigns the key on insert (e.g. an identity/auto-increment column).
    /// A sentinel of the key's default value is registered, so that entities created with
    /// an unset identifier are recognized by EF Core as requiring a generated value.
    /// </summary>
    Database,

    /// <summary>
    /// The application assigns the key before insert, and the value is always sent to the database as-is.
    /// </summary>
    Client
}
