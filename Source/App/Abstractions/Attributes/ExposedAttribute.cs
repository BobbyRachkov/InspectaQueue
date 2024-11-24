namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(System.AttributeTargets.Property | AttributeTargets.Field)]
public class ExposedAttribute : Attribute
{
    public string? DisplayName;
    public string? ToolTip;
}