namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IQueueProvider : ICanSubscribe, IAsyncDisposable
{
    Guid InstanceId { get; }
    IProviderDetails Details { get; }
    IQueueProviderSettings Settings { get; }
}