using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public class Provider
{
    private int _unidentifiedNumber = 0;
    private IQueueProvider _providerInstance;

    public Provider(IQueueProvider provider)
    {
        _providerInstance = provider;
    }

    public QueueType QueueType => _providerInstance.Details.Type;

    public Type Type => _providerInstance.GetType();

    private Dictionary<string, IQueueProvider> VersionsInternal { get; } = new();

    public string ComparableName => GetComparableName(Type);

    public string DisplayName => $"{_providerInstance.Details.Name}";

    public IReadOnlyDictionary<string, IQueueProvider> Versions => VersionsInternal;

    public void Register(IQueueProvider provider)
    {
        if (!VersionsInternal.TryAdd(GetVersion(provider.GetType()), provider))
        {
            return;
        }
    }

    public bool IsMatch(IQueueProvider provider)
    {
        return IsMatch(provider.GetType());
    }

    public bool IsMatch(Type providerType)
    {
        return GetComparableName(providerType) == ComparableName;
    }

    private static string GetComparableName(Type provider)
    {
        return $"{provider.FullName}:{provider.Name}";
    }

    private string GetVersion(Type provider)
    {
        return provider.Assembly.GetName().Version?.ToString() ?? $"unidentified{++_unidentifiedNumber}";
    }
}