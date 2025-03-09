using System.Threading.Channels;

namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface IQueueProvider
{
    IQueueProviderSettings Settings { get; }

    ChannelReader<MessageFrame> Messages { get; }

    Task Connect();

    Task Disconnect();

    Task<bool> TryAcknowledge(MessageFrame frame);
}