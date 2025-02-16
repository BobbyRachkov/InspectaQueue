namespace AutoUpdater.Migrations.Models.Config.v1;

internal class SourceSettingEntryDto
{
    public required string PropertyName { get; set; }
    public object? Value { get; set; }
}