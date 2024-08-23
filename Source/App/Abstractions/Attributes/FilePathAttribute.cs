namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FilePathAttribute : Attribute
{
    public string AllowedExtensions = "(*)";
}