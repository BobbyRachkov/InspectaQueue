namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface ICanAcknowledge
{
    /// <summary>
    /// Tries to acknowledge a sequence of messages.
    /// </summary>
    /// <param name="messages">Collection of messages</param>
    /// <returns>True if all of them succeed; False if at least one failed</returns>
    Task<bool> TryAcknowledge(IEnumerable<IInboundMessage> messages);

    /// <summary>
    /// Tries to negative acknowledge a sequence of messages.
    /// </summary>
    /// <param name="messages">Collection of messages</param>
    /// <returns>True if all of them succeed; False if at least one failed</returns>
    Task<bool> TryNegativeAcknowledge(IEnumerable<IInboundMessage> messages);
}