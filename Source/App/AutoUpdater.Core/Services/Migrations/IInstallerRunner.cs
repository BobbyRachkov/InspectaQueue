using AutoUpdater.Migrations.Models.Interfaces;
using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public interface IInstallerRunner
{
    Task<bool> TryInstallPrerequisiteIfNeeded(
        IPrerequisite prerequisite,
        AbsolutePath tempDirectory,
        CancellationToken cancellationToken = default);
}