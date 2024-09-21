namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(System.AttributeTargets.Property)]
public class ExposedAttribute : Attribute
{
    public string? DisplayName;
    public string? ToolTip;
}