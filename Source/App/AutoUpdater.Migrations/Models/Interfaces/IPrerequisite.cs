namespace AutoUpdater.Migrations.Models.Interfaces;

public interface IPrerequisite
{
    IProcedure WindowsProcedure { get; init; }
    IProcedure? LinuxProcedure { get; init; }
    IProcedure? MacProcedure { get; init; }
}