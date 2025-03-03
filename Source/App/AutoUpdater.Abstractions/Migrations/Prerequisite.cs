using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;

public class Prerequisite : IPrerequisite
{
    public required IProcedure WindowsProcedure { get; init; }
    public IProcedure? LinuxProcedure { get; init; }
    public IProcedure? MacProcedure { get; init; }
}