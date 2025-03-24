using Rachkov.InspectaQueue.Abstractions.Messaging.Models;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IProviderDetails
{
    string Name { get; }
    QueueType Type { get; }
    string Description { get; }
    string PackageVendorName { get; }
}