using System.Threading.Channels;

namespace Rachkov.InspectaQueue.Abstractions;

public interface IQueueProvider
{
    IQueueProviderSettings Settings { get; }

    Task UpdateSettings(IQueueProviderSettings settings);

    ChannelReader<MessageFrame> Messages { get; }

    Task Connect();

    Task Disconnect();

    Task<bool> TryAcknowledge(MessageFrame frame);
}