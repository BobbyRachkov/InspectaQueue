using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class MenuViewModel : ViewModel
{
    private readonly IConfigStoreService _configService;
    private bool _isAutoupdaterEnabled;
    private bool _isBetaReleaseChannel;

    public MenuViewModel(IConfigStoreService configService)
    {
        _configService = configService;
        _isAutoupdaterEnabled = configService.GetSettings().IsAutoUpdaterEnabled;
        _isBetaReleaseChannel = configService.GetSettings().IsAutoUpdaterBetaReleaseChannel;
    }


    public bool IsAutoupdaterEnabled
    {
        get => _isAutoupdaterEnabled;
        set
        {
            _isAutoupdaterEnabled = value;
            _configService.UpdateAndStore(s => s.IsAutoUpdaterEnabled = value);
        }
    }

    public bool IsBetaReleaseChannel
    {
        get => _isBetaReleaseChannel;
        set
        {
            _isBetaReleaseChannel = value;
            _configService.UpdateAndStore(s => s.IsAutoUpdaterBetaReleaseChannel = value);
        }
    }
}