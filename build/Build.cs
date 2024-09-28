using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

public class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile, x => x.CleanPdbs);

    const bool IsRelease = true;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = !IsLocalBuild || IsRelease ? Configuration.Release : Configuration.Debug;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    string AppVersion => "0.1.0.0";

    Dictionary<Project, string> ProviderVersions => new()
    {
        {Solution.QueueProviders.PulsarProvider,"0.1.0.0"}
    };

    AbsolutePath WpfCompileDirectory => ArtifactsDirectory / "wpf";
    AbsolutePath ProvidersCompileDirectory => ArtifactsDirectory / "providers";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ProvidersDirectory => WpfCompileDirectory / "Providers";
    AbsolutePath CompiledAppName => WpfCompileDirectory / "WpfDesktopApp.exe";
    AbsolutePath ProdAppName => WpfCompileDirectory / "InspectaQueue.exe";
    AbsolutePath ProdZipName => ArtifactsDirectory / $"InspectaQueue_{AppVersion}.zip";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(_ => _
                .SetOutput(WpfCompileDirectory)
                .SetProject(Solution.WpfDesktopApp));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(_ => _
                .SetProjectFile(Solution.WpfDesktopApp));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Triggers(CompileProviders)
        .Executes(() =>
        {
            ArtifactsDirectory.DeleteDirectory();

            DotNetTasks.DotNetBuild(_ => _
                .SetProjectFile(Solution.WpfDesktopApp)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(AppVersion)
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
            foreach (var file in Directory.GetFiles(WpfCompileDirectory, "*.pdb", SearchOption.AllDirectories))
            {
                ((AbsolutePath)file).DeleteFile();
            }
        });

    Target Zip => _ => _
        .DependsOn(CompileProviders)
        .DependsOn(CleanPdbs)
        .Executes(() =>
        {
            WpfCompileDirectory.ZipTo(ProdZipName);
        });
}
