using Rachkov.InspectaQueue.Abstractions.Attributes;

namespace Rachkov.InspectaQueue.Providers.Pulsar.Models;

public enum SubscriptionInitialPosition
{
    [Exposed]
    Earliest,

    [Exposed]
    Latest
}