using System.Threading.Channels;

namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface IQueueProvider
{
    string Name { get; }
    string PackageVendorName { get; }

    IQueueProviderSettings Settings { get; }

    Task UpdateSettings(IQueueProviderSettings settings);

    ChannelReader<MessageFrame> Messages { get; }

    Task Connect();

    Task Disconnect();

    Task<bool> TryAcknowledge(MessageFrame frame);
}