using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;

public abstract class MigrationBase : IMigration
{
    public abstract (int major, int minor, int patch) AppVersion { get; }
    public abstract bool ClearAllProviders { get; }
    public virtual bool ForceUseLatestProviderVersionWithoutDeletingProviders => true;
    public abstract bool KeepOnlyLatestProviderVersion { get; }
    public abstract Func<string, string>? MigrateConfig { get; }

    public abstract IPrerequisite[] Prerequisites { get; init; }

    public string PerformConfigMigration(string config)
    {
        var newConfig = MigrateConfig?.Invoke(config) ?? config;

        if (ForceUseLatestProviderVersionWithoutDeletingProviders)
        {
            newConfig = MigrateToLatestProviderVersion(newConfig);
        }

        return UpdateAppVersion(newConfig);
    }

    protected virtual string MigrateToLatestProviderVersion(string newConfig)
    {
        try
        {
            var jsonNode = JsonNode.Parse(newConfig);

            if (jsonNode is null)
            {
                return newConfig;
            }

            foreach (var source in jsonNode["Sources"]!.AsArray())
            {
                var providerTypeString = source!["ProviderType"]!.AsValue().ToString();

                if (providerTypeString.Count(x => x == ':') != 2)
                {
                    continue;
                }

                var lastColonIndex = providerTypeString.LastIndexOf(':');

                if (lastColonIndex >= 0)
                {
                    source!["ProviderType"] = providerTypeString.Substring(0, lastColonIndex);
                }
                else
                {
                    source!["ProviderType"] = providerTypeString;
                }
            }

            return jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return newConfig;
        }
    }

    private string UpdateAppVersion(string newConfig)
    {
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