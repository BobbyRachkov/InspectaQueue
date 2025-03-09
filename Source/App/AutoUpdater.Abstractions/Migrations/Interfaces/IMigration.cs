namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

public interface IMigration
{
    /// <summary>
    /// Version at which the migration has to be applied.
    /// </summary>
    (int major, int minor, int patch) AppVersion { get; }

    /// <summary>
    /// Flag whether to clear all previous providers.
    /// </summary>
    bool ClearAllProviders { get; }

    /// <summary>
    /// Flag whether to migrate all sources to the newest provider version without deleting any providers.
    /// </summary>
    bool ForceUseLatestProviderVersionWithoutDeletingProviders { get; }

    /// <summary>
    /// Flag whether to clear all old provider versions on installation. Keeps the latest one only per provider.
    /// </summary>
    bool KeepOnlyLatestProviderVersion { get; }

    /// <summary>
    /// Customized function that is executed to migrate the schema of the config file. Takes the old file json and has to return the new json.
    /// </summary>
    Func<string, string>? MigrateConfig { get; }

    /// <summary>
    /// List of prerequisites to be installed on the host machine.
    /// </summary>
    IPrerequisite[] Prerequisites { get; }

    /// <summary>
    /// The main method to be called when migrating the schema of the config file. Takes the old file json and has to return the new json.
    /// </summary>
    string PerformConfigMigration(string config);
}