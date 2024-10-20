using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class ProviderViewModel(Provider associatedProvider)
{
    public Provider AssociatedProvider { get; } = associatedProvider;

    public string Name => AssociatedProvider.DisplayName;
}