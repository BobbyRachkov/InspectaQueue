namespace Rachkov.InspectaQueue.Abstractions;

public interface IQueueProviderSettings
{
    string Name { get; }

    string Description { get; }

    /// <summary>
    /// Represents the number of messages after which they will start disappearing from
    /// the list. They will not get deleted from the queue if not explicitly stated.
    /// </summary>
    int HideMessagesAfter { get; set; }
    
    bool AcknowledgeOnReceive { get; set; }
}