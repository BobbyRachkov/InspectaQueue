namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FilePathAttribute : Attribute
{
    /// <summary>
    /// Filter must be in the following syntax "ExplanationFilter1|Filter1; ExplanationFilter2|Filter2" e.g: "JSON files|*.json;"
    /// </summary>
    public string? AllowedExtensions;
    public string? Title;
}