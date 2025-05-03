using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class TestRabbit : IQueueProvider
{
    public Task Connect(IMessageReceiver messageReceiver, IProgressNotificationService progressNotificationService)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectSubscriber()
    {
        throw new NotImplementedException();
    }

    public Task<bool> TryAcknowledge(IInboundMessage message)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Description = "Test description",
        Name = "Rabbit test provider",
        Type = QueueType.RabbitMQ,
        PackageVendorName = "InspectaQueue"
    };

    public IQueueProviderSettings Settings { get; } = new PulsarConsumerSettings();
}