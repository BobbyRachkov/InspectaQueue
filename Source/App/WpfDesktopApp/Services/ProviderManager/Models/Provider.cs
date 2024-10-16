using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public class Provider
{
    private int _unidentifiedNumber = 0;
    private IQueueProvider _providerInstance;

    public Provider(IQueueProvider provider)
    {
        _providerInstance = provider;
    }

    public Type Type => _providerInstance.GetType();

    private Dictionary<string, IQueueProvider> Versions { get; } = new();

    public string ComparableName => GetComparableName(Type);

    public string DisplayName => $"{_providerInstance.Settings.Name}";

    public void Register(IQueueProvider provider)
    {
        if (!Versions.TryAdd(GetVersion(provider.GetType()), provider))
        {
            return;
        }
    }

    public bool IsMatch(IQueueProvider provider)
    {
        return GetComparableName(provider.GetType()) == ComparableName;
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