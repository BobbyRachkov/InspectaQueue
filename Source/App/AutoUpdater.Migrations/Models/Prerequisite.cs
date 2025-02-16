using AutoUpdater.Migrations.Models.Interfaces;

namespace AutoUpdater.Migrations.Models;

public class Prerequisite : IPrerequisite
{
    public required IProcedure WindowsProcedure { get; init; }
    public IProcedure? LinuxProcedure { get; init; }
    public IProcedure? MacProcedure { get; init; }
}