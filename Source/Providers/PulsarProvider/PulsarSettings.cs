using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarSettings : IQueueProviderSettings
{
    public string Name => "Apache Pulsar";
    public string Description => "Pulsar Queue Provider";

    [Exposed(DisplayName = "Issuer URL")]
    public string IssuerUrl { get; set; } = string.Empty;

    [Exposed(DisplayName = nameof(Audience))]
    public string Audience { get; set; } = string.Empty;

    [Exposed(DisplayName = "Service URL")]
    public string ServiceUrl { get; set; } = string.Empty;

    [Exposed(DisplayName = "Topic name")]
    public string TopicName { get; set; } = string.Empty;

    [Exposed(DisplayName = "Subscription Type")]
    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Exclusive;

    [Exposed(DisplayName = "Subscription name")]
    public string SubscriptionName { get; set; } = string.Empty;

    [Exposed(DisplayName = "Authentication file path")]
    [FilePath(AllowedExtensions = "*.json")]
    [SensitiveData]
    public string FilePath { get; set; } = string.Empty;

    [Exposed(
        DisplayName = "Max messages to show",
        ToolTip = "Hides the messages after the desired threshold is reached")]
    public int HideMessagesAfter { get; set; } = 1000;

    [Exposed(
        DisplayName = "Auto acknowledge on receive")]
    public bool AcknowledgeOnReceive { get; set; } = false;
}