using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SourceViewModel : ViewModel
{
    private readonly IQueueProvider _provider;
    private string _name;

    public SourceViewModel(
        Guid id,
        string name,
        IQueueProvider provider,
        SettingEntryViewModel[] settings)
    {
        Id = id;
        _provider = provider;
        _name = name;
        Settings = settings;
    }

    public Guid Id { get; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public Type ProviderType => _provider.GetType();
    public IQueueProvider ProviderInstance => _provider;

    public SettingEntryViewModel[] Settings { get; }


}