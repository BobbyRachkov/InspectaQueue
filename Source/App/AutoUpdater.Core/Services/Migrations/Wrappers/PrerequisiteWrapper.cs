using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations.Wrappers;

internal class PrerequisiteWrapper : IPrerequisite
{
    public PrerequisiteWrapper(dynamic instance)
    {
        WindowsProcedure = new ProcedureWrapper(instance.WindowsProcedure);
        LinuxProcedure = LinuxProcedure is null ? null : new ProcedureWrapper(instance.LinuxProcedure);
        MacProcedure = MacProcedure is null ? null : new ProcedureWrapper(instance.MacProcedure);
    }

    public IProcedure WindowsProcedure { get; init; }
    public IProcedure? LinuxProcedure { get; init; }
    public IProcedure? MacProcedure { get; init; }
}