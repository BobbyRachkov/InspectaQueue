using Newtonsoft.Json;
using Rachkov.InspectaQueue.AutoUpdater.Core.Config;
using Rachkov.InspectaQueue.AutoUpdater.Core.Config.Migrators;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public sealed class UpdateMigratorService : IUpdateMigratorService
{
    private readonly Dictionary<string, Type> _versionedContracts = new()
    {
        {"v1",typeof(Config.v1.SettingsDto)}
    };

    private readonly Dictionary<string, IMigrator> _migrators = new()
    {
    };

    public void MigrateConfig()
    {
        File.Copy(Constants.Path.Config, Constants.Path.MigratedConfig,true);

        var fileContents = File.ReadAllText(Constants.Path.MigratedConfig);
        var parsedVersion = JsonConvert.DeserializeObject<Base>(fileContents);

        if (parsedVersion?.ConfigVersion is null
            || parsedVersion.ConfigVersion == _versionedContracts.Last().Key
            || !_migrators.TryGetValue(parsedVersion.ConfigVersion, out var migrator))
        {
            return;
        }

        var migratedJson = migrator.MigrateJson(fileContents);
        File.WriteAllText(Constants.Path.MigratedConfig, migratedJson);
    }

    public void MigrateProviders()
    {
        Helpers.CopyDirectory(Constants.Path.ProvidersFolder, Constants.Path.MigratedProvidersFolder, true);
    }
}