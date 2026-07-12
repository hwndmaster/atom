namespace Genius.Atom.Data.Ef.Backup;

/// <summary>
/// Configuration for the SQLite database backup behavior, bound from the
/// <see cref="SectionName"/> configuration section.
/// </summary>
public sealed class DatabaseBackupOptions
{
    /// <summary>
    /// The configuration section the options are bound from.
    /// </summary>
    public const string SectionName = "Database:Backup";

    /// <summary>
    /// When <c>false</c>, no backups are created (neither scheduled nor pre-migration).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of backup files to retain. Older backups beyond this count are pruned.
    /// </summary>
    public int MaxBackups { get; set; } = 5;

    /// <summary>
    /// Minimum number of days between scheduled backups. A scheduled backup is taken only when
    /// the most recent backup is older than this many days (weekly by default).
    /// </summary>
    public int IntervalDays { get; set; } = 7;
}
