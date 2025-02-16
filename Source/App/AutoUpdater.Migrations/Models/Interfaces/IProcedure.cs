namespace Rachkov.InspectaQueue.AutoUpdater.Migrations.Models.Interfaces;

public interface IProcedure
{
    Func<bool> HasToBePerformed { get; init; }
    public string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}