using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Models;

public class ProviderDetails : IProviderDetails
{
    public required string Name { get; init; }
    public required QueueType Type { get; init; } = QueueType.Unknown;
    public required string Description { get; init; }
    public required string PackageVendorName { get; init; }
}