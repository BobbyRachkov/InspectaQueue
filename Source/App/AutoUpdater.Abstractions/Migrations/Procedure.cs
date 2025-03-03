using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;

public class Procedure : IProcedure
{
    public required Func<bool> HasToBePerformed { get; init; }
    public required string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}