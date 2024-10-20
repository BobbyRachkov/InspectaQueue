namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

public class SourceDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string ProviderType { get; set; }
    public SourceSettingEntryDto[] Settings { get; set; } = Array.Empty<SourceSettingEntryDto>();
}