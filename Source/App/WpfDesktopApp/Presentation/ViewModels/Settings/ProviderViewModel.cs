using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class ProviderViewModel
{

    public ProviderViewModel()
    {
        Versions = new();
        DisplayableVersions = new();
    }


    private Dictionary<string, IQueueProvider> Versions { get; }

    public List<string> DisplayableVersions { get; private set; }
    public string? SelectedVersion { get; set; }
    public IQueueProvider? SelectedInstance => SelectedVersion is not null ? Versions[SelectedVersion] : null;



    public void Register(IQueueProvider provider)
    {
        if (!Versions.TryAdd(GetVersion(provider.GetType()), provider))
        {
            return;
        }

        DisplayableVersions = Versions.Select(x => x.Key).OrderByDescending(x => x).ToList();
        SelectedVersion = DisplayableVersions.FirstOrDefault();
    }
}