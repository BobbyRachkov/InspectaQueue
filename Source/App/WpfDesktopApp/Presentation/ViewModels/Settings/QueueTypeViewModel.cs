using Rachkov.InspectaQueue.Abstractions.Messaging.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public record QueueTypeViewModel
{
    public string Name { get; }
    public string IconSource { get; }
    public QueueType Type { get; }

    public QueueTypeViewModel(QueueType queueType)
    {
        Name = MapName(queueType);
        IconSource = MapIconSource(queueType);
        Type = queueType;
    }

    private string MapName(QueueType queueType)
    {
        return queueType switch
        {
            QueueType.Unknown => "Unknown",
            QueueType.Other => "Other",
            QueueType.Pulsar => "Apache Pulsar",
            QueueType.Kafka => "Apache Kafka",
            QueueType.RabbitMQ => "RabbitMQ",
            QueueType.Ably => "Ably",
            _ => "Unknown"
        };
    }

    private string MapIconSource(QueueType queueType)
    {
        return queueType switch
        {
            QueueType.Unknown => "../../Resources/icon-unknown-mq.png",
            QueueType.Other => "../../Resources/icon-other-mq.png",
            QueueType.Pulsar => "../../Resources/icon-apache-pulsar.png",
            QueueType.Kafka => "../../Resources/icon-apache-kafka.png",
            QueueType.RabbitMQ => "../../Resources/icon-rabbitmq.png",
            QueueType.Ably => "../../Resources/icon-ably.png",
            _ => "../../Resources/icon-unknown-mq.png"
        };
    }
}