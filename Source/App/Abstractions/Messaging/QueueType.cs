namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public enum QueueType
{
    Unknown,
    Other,
    Pulsar,
    RabbitMQ,
    Kafka,
    Ably
}