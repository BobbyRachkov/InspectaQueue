namespace Rachkov.InspectaQueue.Abstractions.Config.v1;

internal class SourceSettingEntryDto
{
    public required string Name { get; set; }
    public string? ToolTip { get; set; }
    public required string PropertyName { get; set; }
    public required Type Type { get; set; }
    public object? Value { get; set; }
}