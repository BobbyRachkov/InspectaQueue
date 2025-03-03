using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using Rachkov.InspectaQueue.AutoUpdater.Migrations.Helpers;

namespace AutoUpdater.Migrations.Migrations;

public class Migration_2_2_0 : MigrationBase
{
    public override (int major, int minor, int patch) AppVersion => (2, 2, 0);
    public override bool ClearAllProviders => true;
    public override bool KeepOnlyLatestProviderVersion => false;
    public override Func<string, string>? MigrateConfig => null;

    public override IPrerequisite[] Prerequisites { get; init; } =
    [
        new Prerequisite
        {
            WindowsProcedure = new Procedure
            {
                HasToBePerformed = () => !DotNetHelper.IsDotNetVersionInstalled("8.0"),
                UrlOfInstaller = "https://download.visualstudio.microsoft.com/download/pr/fc8c9dea-8180-4dad-bf1b-5f229cf47477/c3f0536639ab40f1470b6bad5e1b95b8/windowsdesktop-runtime-8.0.13-win-x64.exe"
            }
        }
    ];
}