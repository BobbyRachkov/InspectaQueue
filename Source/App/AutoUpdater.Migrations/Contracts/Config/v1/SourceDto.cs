namespace Rachkov.InspectaQueue.AutoUpdater.Migrations.Contracts.Config.v1;

internal class SourceDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string ProviderType { get; set; }
    public SourceSettingEntryDto[] Settings { get; set; } = [];
}