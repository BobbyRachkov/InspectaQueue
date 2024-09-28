namespace Rachkov.InspectaQueue.Abstractions.Config.Migrators;

internal interface IMigrator
{
    string MigrateJson(string content);
}