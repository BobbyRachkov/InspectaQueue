namespace AutoUpdater.Migrations.Models.Interfaces;

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
    /// Flag whether to clear all old provider versions on installation. Keeps the latest one only per provider.
    /// </summary>
    bool KeepOnlyLatestProviderVersion { get; }

    /// <summary>
    /// List of prerequisites to be installed on the host machine.
    /// </summary>
    IPrerequisite[] Prerequisites { get; }
}