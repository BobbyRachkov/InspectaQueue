using System.Collections.Specialized;
using System.Reflection;
using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class ProviderViewModel
{
    private int _unidentifiedNumber = 0;

    public ProviderViewModel(IQueueProvider providerInstance)
    {
        ProviderInstance = providerInstance;
        Versions = new();
        DisplayableVersions = new();
    }

    public Type Type => ProviderInstance.GetType();
    public IQueueProvider ProviderInstance { get; }
    public string ComparableName => GetComparableName(Type);
    public string ProviderName => Type.Name;
    public string DisplayName => $"{ProviderInstance.Settings.Name}";

    private Dictionary<string, IQueueProvider> Versions { get; }

    public List<string> DisplayableVersions { get; private set; }
    public string? SelectedVersion { get; set; }
    public IQueueProvider? SelectedProvider => SelectedVersion is not null ? Versions[SelectedVersion] : null;


    public bool IsMatch(IQueueProvider provider)
    {
        return GetComparableName(provider.GetType()) == ComparableName;
    }

    public void Register(IQueueProvider provider)
    {
        if (!Versions.TryAdd(GetVersion(provider.GetType()), provider))
        {
            return;
        }

        DisplayableVersions = Versions.Select(x => x.Key).OrderByDescending(x => x).ToList();
        SelectedVersion = DisplayableVersions.FirstOrDefault();
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