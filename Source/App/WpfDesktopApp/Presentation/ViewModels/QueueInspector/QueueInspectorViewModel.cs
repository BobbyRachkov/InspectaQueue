using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Messaging;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProgressNotification;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel, IDisposable, ICanBeTopmost
{
    private const double FirstMessageDelayMilliseconds = 2000;
    private const double RemainingMessagesDelayMilliseconds = 0.7;

    private readonly IQueueProvider _queueProvider;
    private readonly ICanPublish? _publisher;
    private readonly CancellationTokenSource _cts = new();

    private bool _topmost;
    private bool? _formatJson;

    private readonly MessageReceiver _messageReceiver;
    private readonly MessageProvider _messageProvider;
    private readonly ProgressNotificationService _receivingProgressNotificationService;
    private readonly ProgressNotificationService _publishingProgressNotificationService;

    private bool _isFirstMessage = true;
    private bool _isMasterLoadingIndicatorOn = true;
    private long _sequence;

    private string _publishKey = string.Empty;
    private string _publishPayload = string.Empty;
    private bool _clearKeyFieldOnPublish = true;
    private bool _clearPayloadFieldOnPublish = true;
    private bool _isPublishingConnected;
    private bool _isPublishPanelOpened;
    private ProgressStatusViewModel? _publishStatusViewModel;

    public QueueInspectorViewModel(
        string? nameSuffix,
        IQueueProvider queueProvider,
        IErrorManager errorManager,
        IWindowManager windowManager)
        : base(errorManager)
    {
        Name = string.IsNullOrWhiteSpace(nameSuffix) ? "Queue Inspector" : $"Queue Inspector | {nameSuffix}";
        _queueProvider = queueProvider;
        //GenerateFakeData();

        _messageReceiver = new MessageReceiver(queueProvider.Settings.HideMessagesAfter + 1, _cts.Token);
        _messageProvider = new MessageProvider(_cts.Token);

        _receivingProgressNotificationService = new ProgressNotificationService();
        _publishingProgressNotificationService = new ProgressNotificationService();

        _receivingProgressNotificationService.MessageDispatched += OnReceivingProgressNotification;
        _publishingProgressNotificationService.MessageDispatched += OnPublishingProgressNotification;

        _messageReceiver.MessageDispatched += MessageReceived;
        queueProvider.Connect(_messageReceiver, _receivingProgressNotificationService);

        _publisher = queueProvider as ICanPublish;

        OnClosing += (_, _) =>
        {
            _cts.Cancel();
            _queueProvider.DisconnectSubscriber();
        };

        EnsureValidMessageOverflowThreshold();

        DisconnectCommand = new RelayCommand(() => windowManager.Close(this));
        PublishCommand = new RelayCommand(Publish);
    }

    #region Receiving

    public ProgressStatusViewModel ReceivingStatusViewModel { get; } = new();

    public ObservableCollection<QueueEntryViewModel> Entries { get; } = [];

    public bool IsMasterLoadingIndicatorOn
    {
        get => _isMasterLoadingIndicatorOn;
        set
        {
            if (value == _isMasterLoadingIndicatorOn) return;
            _isMasterLoadingIndicatorOn = value;
            OnPropertyChanged();
        }
    }

    private void MessageReceived(object? sender, IInboundMessage message)
    {
        var delay = _isFirstMessage ? FirstMessageDelayMilliseconds : RemainingMessagesDelayMilliseconds;
        Task.Delay(TimeSpan.FromMilliseconds(delay)).ContinueWith((t) =>
        {
            _isFirstMessage = false;
            OnUiThread(() =>
            {
                var entry = new QueueEntryViewModel(_sequence++, message);
                IsMasterLoadingIndicatorOn = false;
                AddMessage(entry);
            });
        }).Wait();
    }

    private void EnsureValidMessageOverflowThreshold()
    {
        if (_queueProvider.Settings.HideMessagesAfter < 1)
        {
            ErrorManager.RaiseError(new Error
            {
                Source = Name,
                Text = $"Messages are set to be hidden when reach {_queueProvider.Settings.HideMessagesAfter} in count.\nNo messages will appear in the view."
            });
        }
    }

    private void AddMessage(QueueEntryViewModel entry, int index = 0)
    {
        entry.FormatJson(FormatJson.ToJsonFormatting());
        lock (Entries)
        {
            OnUiThread(() =>
            {
                Entries.Insert(index, entry);
            });
            FeatureStatusUpdated += entry.OnFeatureStatusUpdated;
        }

        RemoveOverflowingMessages();
    }

    private void RemoveOverflowingMessages()
    {
        lock (Entries)
        {
            while (Entries.Count > _queueProvider.Settings.HideMessagesAfter)
            {
                RemoveMessage(Entries[^1]);
            }
        }
    }

    private void RemoveMessage(QueueEntryViewModel entry)
    {
        lock (Entries)
        {
            OnUiThread(() =>
            {
                Entries.Remove(entry);
            });
            FeatureStatusUpdated -= entry.OnFeatureStatusUpdated;
        }

    }

    private void OnReceivingProgressNotification(object? sender, Abstractions.Notifications.ProgressStatus.IProgressNotification e)
    {
        ReceivingStatusViewModel.Update(e);

        if (ReceivingStatusViewModel.Status is Status.Failed)
        {
            IsMasterLoadingIndicatorOn = false;
        }
    }

    #endregion

    #region Publishing

    [MemberNotNullWhen(true, nameof(_publisher))]
    public bool CanPublish => _publisher is not null;

    public string PublishKey
    {
        get => _publishKey;
        set
        {
            _publishKey = value;
            OnPropertyChanged();
        }
    }

    public string PublishPayload
    {
        get => _publishPayload;
        set
        {
            _publishPayload = value;
            OnPropertyChanged();
        }
    }

    public bool ClearKeyFieldOnPublish
    {
        get => _clearKeyFieldOnPublish;
        set
        {
            if (value == _clearKeyFieldOnPublish) return;
            _clearKeyFieldOnPublish = value;
            OnPropertyChanged();
        }
    }

    public bool ClearPayloadFieldOnPublish
    {
        get => _clearPayloadFieldOnPublish;
        set
        {
            if (value == _clearPayloadFieldOnPublish) return;
            _clearPayloadFieldOnPublish = value;
            OnPropertyChanged();
        }
    }

    public bool IsPublishPanelOpened
    {
        get => _isPublishPanelOpened;
        set
        {
            _isPublishPanelOpened = value;
            OnPropertyChanged();
            PublishPanelStateChanged();
        }
    }

    public ProgressStatusViewModel? PublishStatusViewModel
    {
        get => _publishStatusViewModel;
        set
        {
            _publishStatusViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand PublishCommand { get; }

    private void PublishPanelStateChanged()
    {
        if (_isPublishingConnected || !IsPublishPanelOpened)
        {
            return;
        }

        _publisher?.ConnectPublisher(_messageProvider, _publishingProgressNotificationService);
        _isPublishingConnected = true;
    }

    private void Publish()
    {
        if (_publisher is null)
        {
            return;
        }

        _messageProvider.Send(new OutboundMessageFrame
        {
            Key = PublishKey,
            Content = PublishPayload
        });

        if (PublishStatusViewModel is not null)
        {
            PublishStatusViewModel.ReceivedMessages++;
        }

        if (ClearKeyFieldOnPublish)
        {
            PublishKey = string.Empty;
        }

        if (ClearPayloadFieldOnPublish)
        {
            PublishPayload = string.Empty;
        }
    }

    private void OnPublishingProgressNotification(object? sender, Abstractions.Notifications.ProgressStatus.IProgressNotification e)
    {
        PublishStatusViewModel ??= new();
        PublishStatusViewModel.Update(e);
    }

    #endregion

    #region Common

    public event EventHandler<FeatureStatusUpdatedEventArgs>? FeatureStatusUpdated;

    public override string Name { get; }

    public bool Topmost
    {
        get => _topmost;
        set
        {
            _topmost = value;
            OnPropertyChanged();
        }
    }

    public ICommand DisconnectCommand { get; }

    public bool? FormatJson
    {
        get => _formatJson;
        set
        {
            _formatJson = value;
            OnPropertyChanged();
            FeatureStatusUpdated?.Invoke(this, new FormatJsonEventArgs
            {
                Feature = Feature.FormatJson,
                State = true,
                Formatting = value.ToJsonFormatting()
            });
        }
    }

    private void GenerateFakeData()
    {
        Task.Run(async () =>
        {
            for (int i = 0; i < 500; i++)
            {
                AddMessage(new(i, new InboundMessageFrame
                {
                    Content =
                        $"{{\r\n          \"PropertyName\": \"IssuerUrl\",\r\n          \"Type\": \"System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\r\n          \"Value\": \"sdfsdf\"\r\n        }}",
                    Id = i.ToString()

                }));
                await Task.Delay(500);
            }
        });
    }

    public void Dispose()
    {
        while (Entries.Count != 0)
        {
            RemoveMessage(Entries[0]);
        }

        _messageReceiver.MessageDispatched -= MessageReceived;
        _receivingProgressNotificationService.MessageDispatched -= OnReceivingProgressNotification;
        _publishingProgressNotificationService.MessageDispatched -= OnPublishingProgressNotification;
        _queueProvider.DisposeAsync();
    }

    #endregion
}