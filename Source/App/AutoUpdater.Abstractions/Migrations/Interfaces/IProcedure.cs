namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

public interface IProcedure
{
    Func<bool> HasToBePerformed { get; init; }
    public string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}