using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel, IDisposable, ICanBeTopmost
{
    private readonly IQueueProvider _queueProvider;
    private readonly IWindowManager _windowManager;
    private readonly Task? _listenerTask;
    private readonly CancellationTokenSource _cts = new();
    private bool _topmost;
    private bool? _formatJson;

    public QueueInspectorViewModel(
        string? nameSuffix,
        IQueueProvider queueProvider,
        IErrorManager errorManager,
        IWindowManager windowManager)
        : base(errorManager)
    {
        Name = string.IsNullOrWhiteSpace(nameSuffix) ? "Queue Inspector" : $"Queue Inspector | {nameSuffix}";
        _queueProvider = queueProvider;
        _windowManager = windowManager;
        Entries = new();
        //GenerateFakeData();
        queueProvider.Connect();

        _listenerTask = Task.Factory.StartNew(
            () => ListenForMessages(_cts.Token),
            TaskCreationOptions.LongRunning);

        OnClosing += (_, _) => _queueProvider.Disconnect();

        DisconnectCommand = new RelayCommand(() => windowManager.Close(this));
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

    public ObservableCollection<QueueEntryViewModel> Entries { get; }

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
                AddMessage(new(i, new MessageFrame
                {
                    Content =
                        $"{{\r\n          \"PropertyName\": \"IssuerUrl\",\r\n          \"Type\": \"System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\r\n          \"Value\": \"sdfsdf\"\r\n        }}",
                    Id = i

                }));
                await Task.Delay(500);
            }
        });
    }

    private async Task ListenForMessages(CancellationToken cancellationToken)
    {
        var sequence = 0;
        while (await _queueProvider.Messages.WaitToReadAsync(cancellationToken))
        {
            while (_queueProvider.Messages.TryRead(out var message))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = new QueueEntryViewModel(sequence++, message);
                AddMessage(entry);
            }
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
            while (Entries.Count >= _queueProvider.Settings.HideMessagesAfter)
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
        _listenerTask?.Dispose();
    }
}