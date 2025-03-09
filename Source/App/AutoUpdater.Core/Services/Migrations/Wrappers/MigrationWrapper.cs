using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations.Wrappers;

internal class MigrationWrapper : IMigration
{
    private readonly dynamic _instance;

    public MigrationWrapper(dynamic instance)
    {
        _instance = instance;
        Prerequisites = ((IEnumerable<dynamic>)_instance.Prerequisites)
            .Select(p => new PrerequisiteWrapper(p))
            .Cast<IPrerequisite>()
            .ToArray();
    }

    public (int major, int minor, int patch) AppVersion => _instance.AppVersion;
    public bool ClearAllProviders => _instance.ClearAllProviders;

    public bool ForceUseLatestProviderVersionWithoutDeletingProviders =>
        _instance.ForceUseLatestProviderVersionWithoutDeletingProviders;
    public bool KeepOnlyLatestProviderVersion => _instance.KeepOnlyLatestProviderVersion;

    public IPrerequisite[] Prerequisites { get; }

    public Func<string, string>? MigrateConfig => _instance.MigrateConfig;

    public string PerformConfigMigration(string config)
    {
        return (string)_instance.PerformConfigMigration(config);
    }
}