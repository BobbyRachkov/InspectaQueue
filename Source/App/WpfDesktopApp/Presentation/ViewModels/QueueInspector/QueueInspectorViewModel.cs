using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Messaging;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProgressNotification;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel, IDisposable, ICanBeTopmost
{
    private readonly IQueueProvider _queueProvider;
    private readonly CancellationTokenSource _cts = new();
    private bool _topmost;
    private bool? _formatJson;
    private readonly MessageReceiver _messageReceiver;
    private readonly ProgressNotificationService _progressNotificationService;
    private long _sequence;
    private bool _isFirstMessage = true;


    public QueueInspectorViewModel(
        string? nameSuffix,
        IQueueProvider queueProvider,
        IErrorManager errorManager,
        IWindowManager windowManager)
        : base(errorManager)
    {
        Name = string.IsNullOrWhiteSpace(nameSuffix) ? "Queue Inspector" : $"Queue Inspector | {nameSuffix}";
        _queueProvider = queueProvider;

        _messageReceiver = new MessageReceiver(queueProvider.Settings.HideMessagesAfter + 1, _cts.Token);
        _progressNotificationService = new ProgressNotificationService();

        //GenerateFakeData();
        queueProvider.Connect(_messageReceiver, _progressNotificationService);

        _messageReceiver.MessageDispatched += MessageReceived;
        _progressNotificationService.MessageDispatched += OnReceivingProgressNotification;

        OnClosing += (_, _) =>
        {
            _cts.Cancel();
            _queueProvider.DisconnectSubscriber();
        };

        EnsureValidMessageOverflowThreshold();

        DisconnectCommand = new RelayCommand(() => windowManager.Close(this));
    }

    private void OnReceivingProgressNotification(object? sender, Abstractions.Notifications.ProgressStatus.IProgressNotification e)
    {
        ProgressStatusViewModel.UpdateReceiving(e);
    }

    private void MessageReceived(object? sender, IInboundMessage message)
    {
        var delay = _isFirstMessage ? 1000 : 0;

        Task.Delay(TimeSpan.FromMilliseconds(delay)).ContinueWith((t) =>
        {
            OnUiThread(() =>
            {
                var entry = new QueueEntryViewModel(_sequence++, message);
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

    public event EventHandler<FeatureStatusUpdatedEventArgs> FeatureStatusUpdated;

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

    public ProgressStatusViewModel ProgressStatusViewModel { get; } = new();
    public ObservableCollection<QueueEntryViewModel> Entries { get; } = [];

    public ICommand DisconnectCommand { get; }

    public bool? FormatJson
    {
        get => _formatJson;
        set
        {
            _formatJson = value;
            OnPropertyChanged();
            FeatureStatusUpdated.Invoke(this, new FormatJsonEventArgs
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

    public void Dispose()
    {
        _messageReceiver.MessageDispatched -= MessageReceived;
        _progressNotificationService.MessageDispatched -= OnReceivingProgressNotification;
        _queueProvider.DisposeAsync();
    }
}