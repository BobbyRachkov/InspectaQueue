using System.Collections.ObjectModel;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class QueueInspectorViewModel : PresenterViewModel
{
    public override string Name => "Queue Inspector";

    public QueueInspectorViewModel()
    {
        Entries = new();
        GenerateFakeData();
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

    public ObservableCollection<QueueEntryViewModel> Entries { get; }
}