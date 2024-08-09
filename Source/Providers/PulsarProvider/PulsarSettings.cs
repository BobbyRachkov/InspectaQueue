using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarSettings : IQueueProviderSettings
{
    public string Name => "Apache Pulsar";
    public string Description => "Pulsar Queue Provider";

    public string IssuerUrl { get; set; }

    public string Audience { get; set; }

    public string ServiceUrl { get; set; }

    public string TopicName { get; set; }

    public string SubscriptionName { get; set; }

    public string FilePath { get; set; }

    public int HideMessagesAfter { get; set; }

    public bool AcknowledgeOnReceive { get; set; }
}