namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public interface IMigrationService
{
    void Init(string? currentVersion);
    Task<bool> InstallPrerequisites(CancellationToken cancellationToken = default);
    Task<bool> MigrateConfigFiles(CancellationToken cancellationToken = default);
    void DeleteProvidersIfNeeded();
    void DeleteOldProviderVersionsIfNeeded();
}