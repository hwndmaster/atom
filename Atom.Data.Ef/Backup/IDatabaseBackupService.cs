namespace Genius.Atom.Data.Ef.Backup;

/// <summary>
/// Creates and prunes backups of the SQLite database.
/// </summary>
public interface IDatabaseBackupService
{
    /// <summary>
    /// Creates a backup of the database immediately (subject to <see cref="DatabaseBackupOptions.Enabled"/>),
    /// then prunes old backups beyond the configured retention.
    /// </summary>
    /// <param name="reason">Human-readable reason for the backup, used for logging.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The full path of the created backup file, or <c>null</c> if no backup was created.</returns>
    Task<string?> CreateBackupAsync(string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a backup only if the most recent backup is older than
    /// <see cref="DatabaseBackupOptions.IntervalDays"/> (or no backup exists yet).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The full path of the created backup file, or <c>null</c> if no backup was created.</returns>
    Task<string?> CreateBackupIfDueAsync(CancellationToken cancellationToken = default);
}
