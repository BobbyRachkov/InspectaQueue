using Rachkov.InspectaQueue.Abstractions.Messaging;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueEntryViewModel : ViewModel
{
    private readonly MessageFrame _message;
    private readonly bool _isValidJson;
    private string _displayableMessageText;

    public QueueEntryViewModel(long number, MessageFrame message)
    {
        _message = message;
        RawMessageText = _message.Content;
        MessageId = _message.Id;
        MessageKey = _message.Key;
        MessageInternalId = number;
        DisplayableMessageText = RawMessageText;

        _isValidJson = RawMessageText.IsValidJson();
    }

    public long MessageInternalId { get; }
    public long? MessageId { get; }

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
}