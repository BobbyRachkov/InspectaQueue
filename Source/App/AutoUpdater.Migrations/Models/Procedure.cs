using Rachkov.InspectaQueue.AutoUpdater.Migrations.Models.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Migrations.Models;

public class Procedure : IProcedure
{
    public required Func<bool> HasToBePerformed { get; init; }
    public required string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}