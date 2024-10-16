using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public class ProviderManager
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

    private List<Provider> ParseProviders(IQueueProvider[] availableProvidersCollection)
    {
        var list = new List<Provider>();
        foreach (var providerInstance in availableProvidersCollection)
        {
            var providerViewModel = list.FirstOrDefault(x => x.IsMatch(providerInstance))
                                    ?? new Provider(providerInstance);

            providerViewModel.Register(providerInstance);
        }

        return list;
    }
}