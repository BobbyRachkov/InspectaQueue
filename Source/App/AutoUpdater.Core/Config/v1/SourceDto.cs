namespace Rachkov.InspectaQueue.Abstractions.Config.v1;

internal class SourceDto
{
    public required string Name { get; set; }
    public required Type ProviderType { get; set; }
    public SourceSettingEntryDto[] Settings { get; set; } = Array.Empty<SourceSettingEntryDto>();
}