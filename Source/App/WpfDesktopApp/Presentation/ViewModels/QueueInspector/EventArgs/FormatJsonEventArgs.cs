using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector.EventArgs;

public class FormatJsonEventArgs : FeatureStatusUpdatedEventArgs
{
    public required JsonFormatting Formatting { get; init; }
}