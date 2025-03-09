﻿namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface IQueueProviderSettings
{
    /// <summary>
    /// Represents the number of messages after which they will start disappearing from
    /// the list. They will not get deleted from the queue if not explicitly stated.
    /// </summary>
    int HideMessagesAfter { get; set; }

    bool AcknowledgeOnReceive { get; set; }
}