namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;

public interface IAsyncCommunicationService<T>
{
    /// <summary>
    /// Fires when item is received, without blocking the sender.
    /// </summary>
    event EventHandler<T>? ItemDispatched;

    /// <summary>
    /// Sends item to subscribers, without waiting for them to finish processing
    /// </summary>
    Task<bool> SendAsync(object sender, T item);
}