using Rachkov.InspectaQueue.Abstractions.Messaging;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public interface IProviderManager
{
    IQueueProvider GetNewInstance(Type providerType);
    IQueueProvider GetNewInstance(IQueueProvider providerInstance);
    IQueueProvider GetNewInstance(Type providerType, IEnumerable<ISettingPack> settings);
    IQueueProvider GetNewInstance(IQueueProvider provider, IEnumerable<ISettingPack> settings);
    IEnumerable<Provider> GetProviders();
    Provider GetProviderByInstance(IQueueProvider instance);
    IEnumerable<IQueueProvider> GetAllProvidersVersions();
    IQueueProvider FillSettings(IQueueProvider provider, IEnumerable<ISettingPack> settings);
    Provider? GetProviderByType(Type providerType);
}