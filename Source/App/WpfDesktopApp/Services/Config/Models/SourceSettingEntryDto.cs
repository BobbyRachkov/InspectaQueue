namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

public class SourceSettingEntryDto
{
    public required string PropertyName { get; set; }
    public required Type Type { get; set; }
    public object? Value { get; set; }
}