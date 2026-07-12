using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Data.Ef.Backup;

/// <summary>
/// Periodically creates a database backup when the most recent one is older than the configured interval.
/// The decision is based on the timestamp of the newest backup file, so the schedule survives restarts.
/// </summary>
internal sealed class DatabaseBackupHostedService : BackgroundService
{
    // How often we check whether a scheduled backup is due. The actual backup cadence is governed by
    // DatabaseBackupOptions.IntervalDays; this only bounds how soon a due backup is noticed.
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(12);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DatabaseBackupOptions _options;
    private readonly ILogger<DatabaseBackupHostedService> _logger;

    public DatabaseBackupHostedService(
        IServiceScopeFactory scopeFactory,
        DatabaseBackupOptions options,
        ILogger<DatabaseBackupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Scheduled database backups are disabled.");
            return;
        }

        // Give the app time to finish startup, migration and seeding before the first check.
        try
        {
            await Task.Delay(StartupDelay, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(CheckInterval);
        try
        {
            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var backupService = scope.ServiceProvider.GetRequiredService<IDatabaseBackupService>();
                    await backupService.CreateBackupIfDueAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Scheduled database backup failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown.
        }
    }
}
