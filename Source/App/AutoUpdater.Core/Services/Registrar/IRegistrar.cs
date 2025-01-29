namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;

public interface IRegistrar
{
    Task<bool> CreateDesktopShortcut(CancellationToken cancellationToken = default);
    bool RegisterAppInProgramsList(Version? appVersion = null);
}