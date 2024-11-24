using Pulsar.Client.Common;

namespace Rachkov.InspectaQueue.Providers.Pulsar.Extensions;

public static class SubscriptionInitialPositionExtensions
{
    public static SubscriptionInitialPosition ToPulsarEnum(this Models.SubscriptionInitialPosition initialPosition)
    {
        return initialPosition switch
        {
            Models.SubscriptionInitialPosition.Earliest => SubscriptionInitialPosition.Earliest,
            Models.SubscriptionInitialPosition.Latest => SubscriptionInitialPosition.Latest,
            _ => throw new ArgumentOutOfRangeException($"'{initialPosition}' is not supported subscription initial position!")
        };
    }
}