namespace Rachkov.InspectaQueue.AutoUpdater.Migrations.Contracts.Config.v1;

internal class SourceDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string ProviderType { get; set; }
    public SourceSettingEntryDto[] Settings { get; set; } = [];
}