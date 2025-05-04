using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace AutoUpdater.Migrations.Migrations;

public class Migration_2_6_0 : MigrationBase
{
    public override (int major, int minor, int patch) AppVersion => (2, 6, 0);
    public override bool ClearAllProviders => true;
    public override bool KeepOnlyLatestProviderVersion => false;

    public override Func<string, string>? MigrateConfig { get; } = oldConfig =>
    {
        return oldConfig.Replace(
            "Rachkov.InspectaQueue.Providers.Pulsar.PulsarProvider:PulsarProvider",
            "Rachkov.InspectaQueue.Providers.Pulsar.PulsarConsumer:PulsarConsumer");
    };

    public override IPrerequisite[] Prerequisites { get; init; } = [];
}