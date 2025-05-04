using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueEntryViewModel : ViewModel
{
    private readonly IInboundMessage _inboundMessage;
    private readonly bool _isValidJson;
    private string _displayableMessageText;

    public QueueEntryViewModel(long number, IInboundMessage inboundMessage)
    {
        _inboundMessage = inboundMessage;
        RawMessageText = _inboundMessage.Content;
        MessageId = _inboundMessage.Id;
        MessageKey = _inboundMessage.Key;
        MessageInternalId = number;
        _displayableMessageText = RawMessageText;

        _isValidJson = RawMessageText.IsValidJson();
    }

    public long MessageInternalId { get; }

    public string? MessageId { get; }

    public string? MessageKey { get; }

    public string RawMessageText { get; }

    public string DisplayableMessageText
    {
        get => _displayableMessageText;
        private set
        {
            _displayableMessageText = value;
            OnPropertyChanged();
        }
    }

    public IInboundMessage MessageInstance => _inboundMessage;

    public bool IsAcknowledged => _inboundMessage.AcknowledgedStatus is AcknowledgeStatus.Acknowledged;
    public bool IsNegativeAcknowledged => _inboundMessage.AcknowledgedStatus is AcknowledgeStatus.NegativeAcknowledged;

    public void OnFeatureStatusUpdated(object? sender, FeatureStatusUpdatedEventArgs args)
    {
        switch (args.Feature)
        {
            case Feature.FormatJson when args is FormatJsonEventArgs { State: true } formatJsonEventArgs:
                FormatJson(formatJsonEventArgs.Formatting);
                break;
        }
    }

    public void FormatJson(JsonFormatting formatting)
    {
        if (!_isValidJson)
        {
            return;
        }

        DisplayableMessageText = formatting switch
        {
            JsonFormatting.Unchanged => RawMessageText,
            JsonFormatting.Indented => RawMessageText.IndentJson(),
            JsonFormatting.Compact => RawMessageText.CompactJson(),
            _ => DisplayableMessageText
        };
    }

    public void RaiseAcknowledgeStatusChanged()
    {
        OnPropertyChanged(nameof(IsAcknowledged));
        OnPropertyChanged(nameof(IsNegativeAcknowledged));
    }
}