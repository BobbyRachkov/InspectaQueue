using System.Numerics;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueEntryViewModel : ViewModel
{
    private readonly MessageFrame _message;

    public QueueEntryViewModel(long number, MessageFrame message)
    {
        _message = message;
        RawMessageText = _message.Content;
        MessageId = _message.Id;
        MessageInternalId = number;
    }

    public long MessageInternalId { get; }
    public long? MessageId { get; }
    public string RawMessageText { get; }
}