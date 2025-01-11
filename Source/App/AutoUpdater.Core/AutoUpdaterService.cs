using Rachkov.InspectaQueue.Abstractions.EventArgs;
using Rachkov.InspectaQueue.Abstractions.Models;
using System.Diagnostics;
using System.Reflection;

namespace Rachkov.InspectaQueue.Abstractions;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDownloadService _downloadService;
    private ReleaseInfo? _releaseInfo;

    public AutoUpdaterService(IHttpClientFactory httpClientFactory, IDownloadService downloadService)
    {
        _httpClientFactory = httpClientFactory;
        _downloadService = downloadService;
    }

    public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    public event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    public async Task<ReleaseInfo?> GetReleaseInfo()
    {
        if (_releaseInfo is null)
        {
            await UpdateReleaseInfo();
        }

        return _releaseInfo;
    }

    public Version GetExecutingAppVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return new Version(fvi.FileVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0");
    }


    //public void RunFinalCopyScript()
    //{
    //    var scriptPath = "..\\restore.bat";
    //    var scriptFullPath = Path.GetFullPath(scriptPath);

    //    File.WriteAllText(scriptPath, Constants.Script.Finalize2);
    //    ProcessStartInfo psi = new ProcessStartInfo
    //    {
    //        FileName = "cmd.exe",
    //        WorkingDirectory = Path.GetDirectoryName(scriptFullPath),
    //        Arguments = "/c " + Path.GetFileName(scriptFullPath),
    //        UseShellExecute = false,
    //        CreateNoWindow = false, //
    //        RedirectStandardOutput = false,
    //        RedirectStandardError = false,
    //        RedirectStandardInput = false,
    //        WindowStyle = ProcessWindowStyle.Normal
    //    };

    //    Process cmdProcess = Process.Start(psi);
    //    cmdProcess.Dispose();
    //}

    private async Task UpdateReleaseInfo()
    {
        _releaseInfo = await _downloadService.FetchReleaseInfoAsync();
    }

    private void RaiseJobStatusChanged(bool isJobRunning, Stage[] stages)
    {
        JobStatusChanged?.Invoke(this, new JobStatusChangedEventArgs
        {
            IsJobRunning = isJobRunning,
            Stages = stages
        });
    }

    private void RaiseStageStatusChanged(Stage stage, StageStatus status)
    {
        StageStatusChanged?.Invoke(this, new StageStatusChangedEventArgs
        {
            Stage = stage,
            Status = status
        });
    }
}