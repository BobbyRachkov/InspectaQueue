using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public interface ISourceReader
{
    IEnumerable<SourceViewModel> ReadSources();
}