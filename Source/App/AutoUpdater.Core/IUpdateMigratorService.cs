namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public interface IUpdateMigratorService
{
    void MigrateConfig();
    void MigrateProviders();
}