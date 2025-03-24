﻿using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Providers.Pulsar.Models;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarSettings : IQueueProviderSettings
{
    [Exposed(DisplayName = "Issuer URL")]
    public string IssuerUrl { get; set; } = string.Empty;

    [Exposed(DisplayName = nameof(Audience))]
    public string Audience { get; set; } = string.Empty;

    [Exposed(DisplayName = "Service URL")]
    public string ServiceUrl { get; set; } = string.Empty;

    [Exposed(DisplayName = "Topic name")]
    public string TopicName { get; set; } = string.Empty;

    [Exposed(DisplayName = "Subscription name")]
    public string SubscriptionName { get; set; } = string.Empty;

    [Exposed(DisplayName = "Subscription type")]
    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Exclusive;

    [Exposed(DisplayName = "Subscription initial position")]
    public SubscriptionInitialPosition SubscriptionInitialPosition { get; set; } = SubscriptionInitialPosition.Latest;

    [Exposed(DisplayName = "Authentication file path")]
    [FilePath(AllowedExtensions = "JSON files|*.json")]
    public string FilePath { get; set; } = string.Empty;

    [Exposed(
        DisplayName = "Max messages to show",
        ToolTip = "Hides the messages after the desired threshold is reached")]
    public int HideMessagesAfter { get; set; } = 1000;

    [Exposed(DisplayName = "Filter by key contents",
        ToolTip = "Messages are shown if key contains the given string. Empty means no filtering.")]
    public string FilterByKey { get; set; } = string.Empty;

    [Exposed(
        DisplayName = "Auto acknowledge on receive")]
    public bool AcknowledgeOnReceive { get; set; } = false;
}