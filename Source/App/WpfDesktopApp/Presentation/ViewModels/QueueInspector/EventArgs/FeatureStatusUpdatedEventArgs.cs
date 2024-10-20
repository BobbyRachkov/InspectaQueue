using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;

public class FeatureStatusUpdatedEventArgs
{
    public required Feature Feature { get; init; }

    public bool State { get; init; }
}