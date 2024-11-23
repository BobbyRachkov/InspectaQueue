using Rachkov.InspectaQueue.Abstractions.Attributes;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public enum SubscriptionType
{
    [Exposed(DisplayName = "Exclusive")]
    Exclusive,

    [Exposed(DisplayName = "Shared")]
    Shared,

    [Exposed(DisplayName = "Key Shared")]
    KeyShared,

    [Exposed(DisplayName = "Some jibbrish")]
    Test,

    NotExposed,

    [Exposed]
    NoName
}