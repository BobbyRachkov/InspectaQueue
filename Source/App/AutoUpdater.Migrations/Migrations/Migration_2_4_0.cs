using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace AutoUpdater.Migrations.Migrations;

public class Migration_2_4_0 : MigrationBase
{
    public override (int major, int minor, int patch) AppVersion => (2, 4, 0);
    public override bool ClearAllProviders => true;
    public override bool KeepOnlyLatestProviderVersion => false;
    public override Func<string, string>? MigrateConfig { get; } = null;
    public override IPrerequisite[] Prerequisites { get; init; } = [];
}