namespace Rachkov.InspectaQueue.AutoUpdater.Core.Config.Migrators;

internal interface IMigrator
{
    string MigrateJson(string content);
}