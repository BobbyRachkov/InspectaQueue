using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public interface IProviderManager
{
    IQueueProvider GetNewInstance(Type providerType);
    IQueueProvider GetNewInstance(IQueueProvider providerInstance);
    IQueueProvider GetNewInstance(Type providerType, IEnumerable<BasicSettingPack> settings);
    IQueueProvider GetNewInstance(IQueueProvider provider, IEnumerable<BasicSettingPack> settings);
    IEnumerable<Provider> GetProviders();
    Provider GetProviderByInstance(IQueueProvider instance);
    IEnumerable<IQueueProvider> GetAllProviderVersions();
    IQueueProvider FillSettings(IQueueProvider provider, IEnumerable<BasicSettingPack> settings);
}