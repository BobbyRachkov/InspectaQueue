namespace Rachkov.InspectaQueue.Abstractions.Messaging.Models;

public enum QueueType
{
    Unknown,
    Other,
    Pulsar,
    Kafka,
    RabbitMQ,
    Ably
}