using System.Collections.ObjectModel;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel, IDisposable
{
    private readonly IQueueProvider _queueProvider;
    public override string Name => "Queue Inspector";
    private Task? _listenerTask;
    private CancellationTokenSource _cts = new();

    public QueueInspectorViewModel(IQueueProvider queueProvider)
    {
        _queueProvider = queueProvider;
        Entries = new();
        //GenerateFakeData();
        queueProvider.Connect();

        _listenerTask = Task.Factory.StartNew(
            () => ListenForMessages(_cts.Token),
            TaskCreationOptions.LongRunning);

        OnClosing += (_, _) => _queueProvider.Disconnect();
    }

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

    public ObservableCollection<QueueEntryViewModel> Entries { get; }

    public void Dispose()
    {
        _listenerTask?.Dispose();
    }
}