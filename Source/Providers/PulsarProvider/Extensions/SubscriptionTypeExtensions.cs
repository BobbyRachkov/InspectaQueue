using Pulsar.Client.Common;

namespace Rachkov.InspectaQueue.Providers.Pulsar.Extensions;

internal static class SubscriptionTypeExtensions
{
    public static SubscriptionType ToPulsarEnum(this Models.SubscriptionType subscriptionType)
    {
        return subscriptionType switch
        {
            Models.SubscriptionType.Exclusive => SubscriptionType.Exclusive,
            Models.SubscriptionType.KeyShared => SubscriptionType.KeyShared,
            Models.SubscriptionType.Shared => SubscriptionType.Shared,
            Models.SubscriptionType.Failover => SubscriptionType.Failover,
            _ => throw new ArgumentOutOfRangeException($"'{subscriptionType}' is not supported subscription type!")
        };
    }
}