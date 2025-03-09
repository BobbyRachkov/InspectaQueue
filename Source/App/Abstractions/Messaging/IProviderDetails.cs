namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface IProviderDetails
{
    string Name { get; }
    QueueType Type { get; }
    string Description { get; }
    string PackageVendorName { get; }
}