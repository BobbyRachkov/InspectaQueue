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

    public static int Main() => Execute<Build>(x => x.Zip);

    const bool IsRelease = true;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = !IsLocalBuild || IsRelease ? Configuration.Release : Configuration.Debug;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [GitVersion]
    readonly GitVersion GitVersion;

    string AppVersion => GitVersion.SemVer;

    Dictionary<Project, string> ProviderVersions => new()
    {
        {Solution.QueueProviders.PulsarProvider,"0.1.0.0"}
    };

    AbsolutePath ZipDirectory => ArtifactsDirectory / "wpf";
    AbsolutePath WpfCompileDirectory => ArtifactsDirectory / "wpf" / "App";
    AbsolutePath AutoUpdaterCompileDirectory => ArtifactsDirectory / "wpf" / "AutoUpdater";
    AbsolutePath ProvidersCompileDirectory => ArtifactsDirectory / "providers";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ProvidersDirectory => WpfCompileDirectory / "Providers";
    AbsolutePath ProdZipName => ArtifactsDirectory / $"InspectaQueue_{AppVersion}.zip";

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
        .Executes(() =>
        {
            ZipDirectory.ZipTo(ProdZipName);
        });
}
