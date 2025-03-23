namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IQueueProvider : ICanSubscribe
{
    IProviderDetails Details { get; }
    IQueueProviderSettings Settings { get; }
}