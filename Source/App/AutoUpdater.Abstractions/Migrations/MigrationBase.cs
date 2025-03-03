using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;

public abstract class MigrationBase : IMigration
{
    public abstract (int major, int minor, int patch) AppVersion { get; }
    public abstract bool ClearAllProviders { get; }
    public abstract bool KeepOnlyLatestProviderVersion { get; }
    public abstract Func<string, string>? MigrateConfig { get; }

    public abstract IPrerequisite[] Prerequisites { get; init; }

    public string PerformConfigMigration(string config)
    {
        var newConfig = MigrateConfig?.Invoke(config) ?? config;

        try
        {
            var jsonNode = JsonNode.Parse(newConfig);

            if (jsonNode is null)
            {
                return newConfig;
            }

            var versionString = $"{AppVersion.major}.{AppVersion.minor}.{AppVersion.patch}";

            jsonNode[nameof(AppVersion)] = versionString;

            return jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return newConfig;
        }
    }
}