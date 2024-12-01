namespace Rachkov.InspectaQueue.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FilePathAttribute(string filter) : Attribute
{
    public string Filter { get; set; } = filter;

    public FilePathAttribute() : this("*.*")
    {
    }
}