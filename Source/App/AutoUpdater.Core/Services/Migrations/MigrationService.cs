using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using Rachkov.InspectaQueue.AutoUpdater.Core.Extensions;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;
using System.Reflection;

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
        if (!_pathsConfiguration.MigrationsDllPath.FileExists())
        {
            return;
        }

        var current = string.IsNullOrWhiteSpace(currentVersion) ? null : new Version(currentVersion);

        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var assembly = LoadMigrationAssembly(_pathsConfiguration.MigrationsDllPath);
            if (assembly is null)
            {
                return;
            }

            var interfaceType = typeof(IMigration);
            var migrations = GetCompatibleMigrations(assembly, interfaceType, current);

            _pendingMigrations.Clear();
            _pendingMigrations.AddRange(migrations);
        }
        catch (Exception)
        {
            _pendingMigrations.Clear();
        }
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        return Assembly.LoadFile(_pathsConfiguration.MigrationsDllPath.Parent / "AutoUpdater.Abstractions.dll");
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

    private static Assembly? LoadMigrationAssembly(string path)
    {
        try
        {
            var assemblyBytes = File.ReadAllBytes(path);
            return Assembly.Load(assemblyBytes);
        }
        catch
        {
            return null;
        }
    }

    private static List<IMigration> GetCompatibleMigrations(Assembly assembly, Type interfaceType, Version? currentVersion)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.FullName == interfaceType.FullName))
            .Select(t =>
            {
                try
                {
                    var instance = assembly.CreateInstance(t.FullName!);
                    if (instance is null)
                    {
                        return null;
                    }

                    // Use dynamic to bypass assembly version mismatch
                    dynamic dynamicInstance = instance;
                    var version = dynamicInstance.AppVersion;

                    // Only create IMigration wrapper if version check passes
                    if (currentVersion is null || new Version(version).CompareTo(currentVersion) > 0)
                    {
                        return new MigrationWrapper(dynamicInstance);
                    }
                }
                catch
                {
                    // Log or handle instantiation errors
                }
                return null;
            })
            .Where(m => m is not null)
            .OrderBy(m => m!.AppVersion.ToVersion())
            .Cast<IMigration>()
            .ToList()!;
    }
}

internal class MigrationWrapper : IMigration
{
    private readonly dynamic _instance;

    public MigrationWrapper(dynamic instance)
    {
        _instance = instance;
        Prerequisites = ((IEnumerable<dynamic>)_instance.Prerequisites)
            .Select(p => new PrerequisiteWrapper(p))
            .Cast<IPrerequisite>()
            .ToArray();
    }

    public (int major, int minor, int patch) AppVersion => _instance.AppVersion;
    public bool ClearAllProviders => _instance.ClearAllProviders;
    public bool KeepOnlyLatestProviderVersion => _instance.KeepOnlyLatestProviderVersion;
    public IPrerequisite[] Prerequisites { get; }
    public Func<string, string>? MigrateConfig => _instance.MigrateConfig;
}

internal class PrerequisiteWrapper : IPrerequisite
{
    private readonly dynamic _instance;

    public PrerequisiteWrapper(dynamic instance)
    {
        _instance = instance;
        WindowsProcedure = new ProcedureWrapper(_instance.WindowsProcedure);
        LinuxProcedure = LinuxProcedure is null ? null : new ProcedureWrapper(_instance.LinuxProcedure);
        MacProcedure = MacProcedure is null ? null : new ProcedureWrapper(_instance.MacProcedure);
    }

    public IProcedure WindowsProcedure { get; init; }
    public IProcedure? LinuxProcedure { get; init; }
    public IProcedure? MacProcedure { get; init; }
}

internal class ProcedureWrapper : IProcedure
{
    private readonly dynamic _instance;

    public ProcedureWrapper(dynamic instance)
    {
        _instance = instance;
        HasToBePerformed = _instance.HasToBePerformed;
        UrlOfInstaller = _instance.UrlOfInstaller;
        InstallerArgs = _instance.InstallerArgs;
    }

    public Func<bool> HasToBePerformed { get; init; }
    public string UrlOfInstaller { get; init; }
    public string? InstallerArgs { get; init; }
}