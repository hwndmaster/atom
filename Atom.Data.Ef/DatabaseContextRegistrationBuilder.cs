using Genius.Atom.Data.Ef.Backup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genius.Atom.Data.Ef;

/// <summary>
/// Allows chaining optional database features onto a
/// <see cref="DatabaseContextRegistration.Register{TDbContext}(IServiceCollection)"/> call.
/// </summary>
public sealed class DatabaseContextRegistrationBuilder<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceCollection _services;

    internal DatabaseContextRegistrationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers SQLite database backups: <see cref="IDatabaseBackupService"/> for on-demand backups,
    /// <see cref="IDatabaseMigrator"/> for the migrate-with-backup startup flow, and a hosted service
    /// performing scheduled backups. Options are bound from the
    /// <see cref="DatabaseBackupOptions.SectionName"/> configuration section; when the section is
    /// absent, defaults apply (enabled, 5 backups retained, 7-day interval).
    /// Requires <typeparamref name="TDbContext"/> to use the SQLite provider.
    /// </summary>
    public DatabaseContextRegistrationBuilder<TDbContext> WithBackup(IConfiguration configuration)
    {
        Genius.Atom.Infrastructure.Guard.NotNull(configuration);

        var backupOptions = configuration.GetSection(DatabaseBackupOptions.SectionName).Get<DatabaseBackupOptions>()
            ?? new DatabaseBackupOptions();
        return WithBackup(backupOptions);
    }

    /// <inheritdoc cref="WithBackup(IConfiguration)"/>
    public DatabaseContextRegistrationBuilder<TDbContext> WithBackup(DatabaseBackupOptions backupOptions)
    {
        Genius.Atom.Infrastructure.Guard.NotNull(backupOptions);

        _services.AddSingleton(backupOptions);
        _services.AddScoped<IDatabaseBackupService, DatabaseBackupService<TDbContext>>();
        _services.AddScoped<IDatabaseMigrator, DatabaseMigrator<TDbContext>>();
        _services.AddHostedService<DatabaseBackupHostedService>();

        return this;
    }
}
