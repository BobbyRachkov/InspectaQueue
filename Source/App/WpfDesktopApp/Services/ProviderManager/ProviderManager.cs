using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public class ProviderManager : IProviderManager
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly List<Provider> _providers;

    public ProviderManager(
        ILifetimeScope lifetimeScope,
        IEnumerable<IQueueProvider> availableProviders)
    {
        _lifetimeScope = lifetimeScope;
        _providers = ParseProviders(availableProviders.ToArray());
    }

    public IQueueProvider GetNewInstance(Type providerType)
    {
        return (IQueueProvider)_lifetimeScope.Resolve(providerType);
    }

    public IQueueProvider GetNewInstance(IQueueProvider providerInstance)
    {
        return GetNewInstance(providerInstance.GetType());
    }

    public IQueueProvider GetNewInstance(Type providerType, IEnumerable<SettingPack> settings)
    {
        return FillSettings(GetNewInstance(providerType), settings);
    }

    public IQueueProvider GetNewInstance(IQueueProvider provider, IEnumerable<SettingPack> settings)
    {
        return FillSettings(GetNewInstance(provider), settings);
    }

    public IQueueProvider FillSettings(IQueueProvider provider, IEnumerable<SettingPack> settings)
    {
        UpdateSettings(provider.Settings, settings);
        return provider;
    }

    public IEnumerable<Provider> GetProviders()
    {
        return _providers;
    }

    public Provider GetProviderByInstance(IQueueProvider instance)
    {
        return _providers.First(x => x.IsMatch(instance));
    }

    public IEnumerable<IQueueProvider> GetAllProviderVersions()
    {
        foreach (var provider in _providers)
        {
            foreach (var version in provider.Versions)
            {
                yield return version.Value;
            }
        }
    }

    private IQueueProviderSettings UpdateSettings(IQueueProviderSettings settingsObjectToUpdate, IEnumerable<SettingPack> settings)
    {
        foreach (var setting in settings)
        {
            setting.ReflectedProperty.SetValue(settingsObjectToUpdate, EnsureProperValueType(setting));
        }


        return settingsObjectToUpdate;
    }

    private object? EnsureProperValueType(SettingPack setting)
    {
        if (setting.Value is null)
        {
            return null;
        }

        if (setting.Type == typeof(int)
            && setting.Value.GetType() != typeof(int))
        {
            return Convert.ToInt32(setting.Value);
        }

        return setting.Value;
    }

    private List<Provider> ParseProviders(IQueueProvider[] availableProvidersCollection)
    {
        var list = new List<Provider>();
        foreach (var providerInstance in availableProvidersCollection)
        {
            var providerViewModel = list.FirstOrDefault(x => x.IsMatch(providerInstance));

            if (providerViewModel is null)
            {
                providerViewModel = new Provider(providerInstance);
                list.Add(providerViewModel);
            }

            providerViewModel.Register(providerInstance);
        }

        return list;
    }
}