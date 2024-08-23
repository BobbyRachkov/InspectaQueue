using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

public interface ISettingsParser
{
    IEnumerable<SettingEntryViewModel> ParseMembers(IQueueProvider queueProvider);
}