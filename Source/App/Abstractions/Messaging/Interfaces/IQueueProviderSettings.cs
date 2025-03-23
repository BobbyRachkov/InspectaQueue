namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IQueueProviderSettings
{
    /// <summary>
    /// Represents the number of messages after which they will start disappearing from
    /// the list. They will not get deleted from the queue if not explicitly stated.
    /// </summary>
    int HideMessagesAfter { get; set; }

    /// <summary>
    /// if set to true. the provider will auto acknowledge the message, as soon as it is received
    /// </summary>
    bool AcknowledgeOnReceive { get; set; }
}