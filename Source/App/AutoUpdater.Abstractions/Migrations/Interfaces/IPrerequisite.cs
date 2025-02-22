namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

public interface IPrerequisite
{
    IProcedure WindowsProcedure { get; init; }
    IProcedure? LinuxProcedure { get; init; }
    IProcedure? MacProcedure { get; init; }
}