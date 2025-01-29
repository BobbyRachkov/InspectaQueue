namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;

public interface IRegistrar
{
    Task<bool> CreateDesktopShortcut(CancellationToken cancellationToken = default);

    Task<bool> RegisterAppInProgramUninstallList(Version? appVersion = null, CancellationToken cancellationToken = default);
}