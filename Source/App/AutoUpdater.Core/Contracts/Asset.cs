using System.Runtime.Serialization;

namespace Rachkov.InspectaQueue.Abstractions.Contracts;

[DataContract]
internal sealed class Asset
{
    [DataMember(Name = "url")]
    public string? Url { get; set; }

    [DataMember(Name = "id")]
    public int? Id { get; set; }

    [DataMember(Name = "node_id")]
    public string? NodeId { get; set; }

    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "label")]
    public object? Label { get; set; }

    [DataMember(Name = "uploader")]
    public Uploader? Uploader { get; set; }

    [DataMember(Name = "content_type")]
    public string? ContentType { get; set; }

    [DataMember(Name = "state")]
    public string? State { get; set; }

    [DataMember(Name = "size")]
    public int? Size { get; set; }

    [DataMember(Name = "download_count")]
    public int? DownloadCount { get; set; }

    [DataMember(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }

    [DataMember(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [DataMember(Name = "browser_download_url")]
    public string? BrowserDownloadUrl { get; set; }
}