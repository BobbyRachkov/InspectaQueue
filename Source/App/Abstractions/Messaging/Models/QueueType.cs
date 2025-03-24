namespace Rachkov.InspectaQueue.Abstractions.Messaging.Models;

public enum QueueType
{
    Unknown,
    Other,
    Pulsar,
    RabbitMQ,
    Kafka,
    Ably
}