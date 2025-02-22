using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public interface IInstallerRunner
{
    Task<bool> TryInstallPrerequisiteIfNeeded(
        IPrerequisite prerequisite,
        AbsolutePath tempDirectory,
        CancellationToken cancellationToken = default);
}