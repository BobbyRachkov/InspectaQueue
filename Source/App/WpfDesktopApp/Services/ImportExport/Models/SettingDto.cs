using System.Runtime.Serialization;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport.Models;

[DataContract]
public class SettingDto
{
    [DataMember(Name = "p")]
    public required string PropertyName { get; set; }

    [DataMember(Name = "v")]
    public object? Value { get; set; }
}