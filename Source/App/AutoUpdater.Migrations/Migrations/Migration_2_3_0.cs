using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace AutoUpdater.Migrations.Migrations;

public class Migration_2_3_0 : MigrationBase
{
    public override (int major, int minor, int patch) AppVersion => (2, 3, 0);
    public override bool ClearAllProviders => false;
    public override bool KeepOnlyLatestProviderVersion => false;
    public override Func<string, string>? MigrateConfig => null;
    public override IPrerequisite[] Prerequisites { get; init; } = [];
}