namespace Rachkov.InspectaQueue.Abstractions;

public interface IUpdateMigratorService
{
    void MigrateConfig();
    void MigrateProviders();
}