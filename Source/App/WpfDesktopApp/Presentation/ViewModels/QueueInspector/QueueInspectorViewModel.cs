using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using System.Collections.ObjectModel;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel, IDisposable, ICanBeTopmost
{
    private readonly IQueueProvider _queueProvider;
    private readonly IWindowManager _windowManager;
    private readonly Task? _listenerTask;
    private readonly CancellationTokenSource _cts = new();
    private bool _topmost;

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

        DisconnectCommand = new(() => windowManager.Close(this));
    }

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

    public RelayCommand DisconnectCommand { get; }

    private void GenerateFakeData()
    {
        for (int i = 0; i < 10; i++)
        {
            Entries.Add(new(i, new MessageFrame
            {
                Content = $"Some jibbrish {i}",
                Id = i
            }));
        }
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
                OnUiThread(() =>
                {
                    Entries.Insert(0, entry);
                });
            }
        }
    }


    public void Dispose()
    {
        _listenerTask?.Dispose();
    }
}