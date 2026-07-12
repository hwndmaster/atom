namespace Genius.Atom.Data.Ef;

/// <summary>
/// Brings the database schema up to date at application startup.
/// </summary>
public interface IDatabaseMigrator
{
    /// <summary>
    /// Applies any pending EF Core migrations, taking a database backup first when at least one
    /// migration is pending. Databases that were originally created with <c>EnsureCreated()</c>
    /// (and therefore lack the migrations history table) are baselined by recording only the
    /// initial migration as applied, so it is never executed against the existing schema while
    /// all later migrations still apply normally.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task MigrateWithBackupAsync(CancellationToken cancellationToken = default);
}
