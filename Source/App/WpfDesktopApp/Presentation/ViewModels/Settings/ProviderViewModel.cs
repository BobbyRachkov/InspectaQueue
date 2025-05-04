using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class ProviderViewModel(Provider associatedProvider)
{
    public Provider AssociatedProvider { get; } = associatedProvider;

    public string Name => AssociatedProvider.DisplayName;
    public string Description => AssociatedProvider.Versions.FirstOrDefault().Value?.Details.Description ?? string.Empty;
    public string Vendor => AssociatedProvider.Versions.FirstOrDefault().Value?.Details.PackageVendorName ?? string.Empty;
    public bool CanPublish => AssociatedProvider.Versions.Any(x => x.Value is ICanPublish);
    public bool CanAcknowledge => AssociatedProvider.Versions.Any(x => x.Value is ICanAcknowledge);
}