using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Core.Extensions;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;
using Rachkov.InspectaQueue.AutoUpdater.Migrations.Models.Interfaces;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public class MigrationService : IMigrationService
{
    private readonly IApplicationPathsConfiguration _pathsConfiguration;
    private readonly IInstallerRunner _installerRunner;
    private readonly List<IMigration> _pendingMigrations = new();

    public MigrationService(
        IApplicationPathsConfiguration pathsConfiguration,
        IInstallerRunner installerRunner)
    {
        _pathsConfiguration = pathsConfiguration;
        _installerRunner = installerRunner;
    }

    public void Init(string? currentVersion)
    {
        //if (!_pathsConfiguration.MigrationsDllPath.FileExists())
        //{
        //    return;
        //}

        var current = string.IsNullOrWhiteSpace(currentVersion) ? null : new Version(currentVersion);
        try
        {
            var interfaceType = typeof(IMigration);
            var migrationsAssembly = interfaceType.Assembly;
            var migrations = migrationsAssembly.GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .Select(t => (IMigration)Activator.CreateInstance(t)!)
                .Where(m => m.AppVersion.ToVersion() > current)
                .OrderBy(m => m.AppVersion.ToVersion())
                .ToList();

            _pendingMigrations.Clear();
            _pendingMigrations.AddRange(migrations);

        }
        catch (Exception ex)
        {
            return;
        }
    }

    public async Task<bool> InstallPrerequisites(CancellationToken cancellationToken = default)
    {
        foreach (var migration in _pendingMigrations)
        {
            foreach (var prerequisite in migration.Prerequisites)
            {
                var tempDirectory = _pathsConfiguration.IqBaseDirectory;

                if (!await _installerRunner.TryInstallPrerequisiteIfNeeded(
                    prerequisite,
                    tempDirectory,
                    cancellationToken))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> MigrateConfigFiles(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_pathsConfiguration.ConfigFilePath.FileExists())
            {
                return true;
            }

            var configJson = await File.ReadAllTextAsync(_pathsConfiguration.ConfigFilePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(configJson))
            {
                return true;
            }

            configJson = _pendingMigrations
                .Where(x => x.MigrateConfig is not null)
                .Aggregate(configJson,
                    (current, migration) => migration.MigrateConfig!.Invoke(current)
                    );

            await File.WriteAllTextAsync(_pathsConfiguration.ConfigFilePath, configJson, cancellationToken);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void DeleteProvidersIfNeeded()
    {
        if (!_pendingMigrations.Any(x => x.ClearAllProviders))
        {
            return;
        }

        _pathsConfiguration.ProvidersDirectory.CreateOrCleanDirectory();
    }

    public void DeleteOldProviderVersionsIfNeeded()
    {
        if (!_pendingMigrations.Any(x => x.KeepOnlyLatestProviderVersion))
        {
            return;
        }

        if (!_pathsConfiguration.ProvidersDirectory.DirectoryExists())
        {
            return;
        }

        var providerDirectories = Directory.GetDirectories(_pathsConfiguration.ProvidersDirectory);
        var providerGroups = providerDirectories
            .Select(dir =>
            {
                var dirName = Path.GetFileName(dir);
                var versionStr = dirName.Split('_').LastOrDefault();
                Version? version = null;

                try
                {
                    if (!string.IsNullOrEmpty(versionStr))
                    {
                        version = new Version(versionStr);
                    }
                }
                catch
                {
                    // Invalid version format, will be null
                }

                return new { Path = dir, Name = dirName, Version = version };
            })
            .Where(x => x.Version is not null)
            .GroupBy(x => x.Name.Split('_')[0]); // Group by provider name without version

        foreach (var group in providerGroups)
        {
            var orderedVersions = group.OrderByDescending(x => x.Version).ToList();

            // Skip the first one (latest version)
            foreach (var oldVersion in orderedVersions.Skip(1))
            {
                try
                {
                    Directory.Delete(oldVersion.Path, recursive: true);
                }
                catch
                {
                    // Continue with other directories if one fails to delete
                }
            }
        }
    }
}