using Rachkov.InspectaQueue.Abstractions.Attributes;

namespace Rachkov.InspectaQueue.Providers.Pulsar.Models;

public enum SubscriptionType
{
    [Exposed]
    Exclusive,

    [Exposed(DisplayName = "Key Shared")]
    KeyShared,

    [Exposed]
    Shared,

    [Exposed]
    Failover
}
