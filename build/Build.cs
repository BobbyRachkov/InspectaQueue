using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Zip, x => x.PackInstaller);

    const bool IsRelease = true;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = !IsLocalBuild || IsRelease ? Configuration.Release : Configuration.Debug;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [GitVersion]
    readonly GitVersion GitVersion;

    string AppVersion => GitVersion.SemVer;

    string InstallerVersion => "1.3.1";
    string MigrationVersion => InstallerVersion;

    Dictionary<Project, string> ProviderVersions => new()
    {
        {Solution.QueueProviders.PulsarProvider,"1.1.0.0"}
    };

    AbsolutePath ZipDirectory => ArtifactsDirectory / "wpf";
    AbsolutePath WpfCompileDirectory => ArtifactsDirectory / "wpf" / "App";
    AbsolutePath AutoUpdaterCompileDirectory => ArtifactsDirectory / "wpf" / "AutoUpdater";
    AbsolutePath ProvidersCompileDirectory => ArtifactsDirectory / "providers";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ProvidersDirectory => ZipDirectory / "Providers";
    AbsolutePath ProdZipName => ArtifactsDirectory / $"InspectaQueue_{AppVersion}.zip";
    AbsolutePath InstallerBuildDirectory => ArtifactsDirectory / $"Installer";
    AbsolutePath InstallerPath => ArtifactsDirectory / $"Installer_{InstallerVersion}.exe";
    AbsolutePath MigrationsCompileDirectory => ArtifactsDirectory / $"migrations";
    AbsolutePath MigrationsCompiledName => MigrationsCompileDirectory / $"AutoUpdater.Migrations.dll";
    AbsolutePath MigrationsAbstractionsCompiledName => MigrationsCompileDirectory / $"AutoUpdater.Abstractions.dll";
    AbsolutePath MigrationsProdName => ZipDirectory / "Migration" / $"Migrations.dll";
    AbsolutePath MigrationsAbstractionsProdName => ZipDirectory / "Migration" / $"Abstractions.dll";


    AbsolutePath ProvidersDevDirectory => RootDirectory / "\\Source\\App\\WpfDesktopApp\\bin\\Debug\\Providers";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(_ => _
                .SetOutput(WpfCompileDirectory)
                .SetProject(Solution.App.WpfDesktopApp));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(Solution.App.WpfDesktopApp));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Triggers(CompileProviders)
        .Executes(() =>
        {
            ArtifactsDirectory.DeleteDirectory();

            DotNetTasks.DotNetBuild(_ => _
                .SetProjectFile(Solution.App.WpfDesktopApp)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(AppVersion)
                .SetInformationalVersion(AppVersion)
                .SetAuthors("Bobi Rachkov")
                .SetOutputDirectory(WpfCompileDirectory)
            );

            ProvidersDirectory.CreateOrCleanDirectory();
        });

    Target PackInstaller => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetPublish(_ => _
                .SetProject(Solution.AutoUpdater.AutoUpdater_App)
                .SetSelfContained(true)
                .SetFramework("net8.0")
                .SetRuntime("win-x64")
                .EnablePublishSingleFile()
                .EnablePublishReadyToRun()
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(InstallerVersion)
                .SetFileVersion(InstallerVersion)
                .SetInformationalVersion(InstallerVersion)
                .SetAuthors("Bobi Rachkov")
                .SetOutput(InstallerBuildDirectory)
            );

            var installer = (AbsolutePath)Directory.GetFiles(InstallerBuildDirectory).Single(x => x.EndsWith(".exe"));
            installer.Copy(InstallerPath);

            InstallerBuildDirectory.DeleteDirectory();
        });

    Target CompileProviders => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            ProvidersCompileDirectory.CreateOrCleanDirectory();

            foreach (var project in Solution.AllProjects.Where(x => x.Name.EndsWith("Provider")))
            {
                Log.Information("Compiling project: {projectName}", project.Name);
                var providerName = project.Name.Replace("Provider", "");
                var providerDirectory = ProvidersCompileDirectory / $"{providerName}_{ProviderVersions[project]}";

                DotNetTasks.DotNetBuild(_ => _
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(ProviderVersions[project])
                    .SetAuthors("Bobi Rachkov")
                    .SetOutputDirectory(providerDirectory)
                );
            }

            ProvidersCompileDirectory.Copy(ProvidersDirectory, ExistsPolicy.MergeAndOverwrite);
            ProvidersCompileDirectory.DeleteDirectory();
        });

    Target CleanPdbs => _ => _
        .After(Compile)
        .After(CompileProviders)
        .Executes(() =>
        {
            foreach (var file in Directory.GetFiles(ArtifactsDirectory, "*.pdb", SearchOption.AllDirectories))
            {
                ((AbsolutePath)file).DeleteFile();
            }
        });

    Target Zip => _ => _
        .DependsOn(CompileProviders)
        .DependsOn(CleanPdbs)
        .DependsOn(CompileMigrations)
        .Executes(() =>
        {
            ZipDirectory.ZipTo(ProdZipName);
            ZipDirectory.DeleteDirectory();
        });

    Target Dev => _ => _
        .DependsOn(DebugProviders);

    Target DebugProviders => _ => _
        .Executes(() =>
        {
            if (Configuration == Configuration.Release)
            {
                Log.Information("Searching providers");
                return;
            }

            ProvidersDevDirectory.CreateOrCleanDirectory();

            Log.Information("Searching providers");

            var projects = Solution.AllProjects.Where(x => x.Name.EndsWith("Provider")).ToList();

            Log.Information("Found projects: {projects}", string.Join(", ", projects.Select(x => x.Name)));

            foreach (var project in projects)
            {
                Log.Information("Compiling project: {projectName}", project.Name);

                var providerName = project.Name.Replace("Provider", "");
                var providerDirectory = ProvidersDevDirectory / $"{providerName}_0.0.0.0-dev";
                providerDirectory.CreateOrCleanDirectory();

                Log.Information("Project path: {providerDirectory}", providerDirectory);

                DotNetTasks.DotNetBuild(_ => _
                    .SetProjectFile(project)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion("0.0.0.0")
                    .SetAuthors("Bobi Rachkov")
                    .SetOutputDirectory(providerDirectory)
                );
            }
        });

    Target CompileMigrations => _ => _
        .After(Compile)
        .Executes(() =>
        {
            var project = Solution.AutoUpdater.AutoUpdater_Migrations;
            MigrationsCompileDirectory.CreateOrCleanDirectory();
            Log.Information("Compiling migrations");

            DotNetTasks.DotNetPublish(_ => _
                .SetProject(project)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(MigrationVersion)
                .SetFileVersion(MigrationVersion)
                .SetInformationalVersion(MigrationVersion)
                .SetAuthors("Bobi Rachkov")
                .SetOutput(MigrationsCompileDirectory)
            );

            MigrationsCompiledName.Copy(MigrationsProdName, ExistsPolicy.FileOverwrite);
            MigrationsAbstractionsCompiledName.Copy(MigrationsAbstractionsProdName, ExistsPolicy.FileOverwrite);
            MigrationsCompileDirectory.DeleteDirectory();
        });
}
