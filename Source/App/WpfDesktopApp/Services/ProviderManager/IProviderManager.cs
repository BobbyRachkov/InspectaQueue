using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public interface IProviderManager
{
    IQueueProvider GetNewInstance(Type providerType);
    IQueueProvider GetNewInstance(IQueueProvider providerInstance);
    IQueueProvider GetNewInstance(Type providerType, IEnumerable<SettingPack> settings);
    IQueueProvider GetNewInstance(IQueueProvider provider, IEnumerable<SettingPack> settings);
    IEnumerable<Provider> GetProviders();
    IEnumerable<IQueueProvider> GetAllProviderVersions();
    IQueueProvider FillSettings(IQueueProvider provider, IEnumerable<SettingPack> settings);
}