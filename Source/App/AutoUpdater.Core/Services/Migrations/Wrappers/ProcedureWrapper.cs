using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations.Wrappers;

internal class ProcedureWrapper : IProcedure
{
    public ProcedureWrapper(dynamic instance)
    {
        HasToBePerformed = instance.HasToBePerformed;
        UrlOfInstaller = instance.UrlOfInstaller;
        InstallerArgs = instance.InstallerArgs;
    }

    public Func<bool> HasToBePerformed { get; init; }
    public string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}