using Newtonsoft.Json;
using Rachkov.InspectaQueue.Abstractions.Contracts;
using Rachkov.InspectaQueue.Abstractions.Extensions;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;

namespace Rachkov.InspectaQueue.Abstractions;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AutoUpdaterService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(Version version, string? downloadUrl)?> GetLatestVersion(ReleaseType releaseType)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Constants.Url.RepositoryApi);
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product", "1"));

        try
        {
            var allReleasesJson = await client.GetStringAsync(Constants.Url.ReleasesPath);
            var allReleases = JsonConvert.DeserializeObject<Release[]>(allReleasesJson);

            if (releaseType is ReleaseType.Official)
            {
                var release = allReleases?.FirstOrDefault(x => x.Prerelease is false);

                if (release?.TagName is null)
                {
                    return null;
                }

                return (new Version(release.TagName), release.GetBrowserDownloadString());
            }

            var prerelease = allReleases?.FirstOrDefault();

            if (prerelease?.TagName is null)
            {
                return null;
            }

            return (new Version(prerelease.TagName), prerelease.GetBrowserDownloadString());
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task DownloadVersion(string downloadUrl)
    {
        var client = _httpClientFactory.CreateClient();
        Stream fileStream = await client.GetStreamAsync(downloadUrl);

        await using FileStream outputFileStream = new FileStream(Constants.Path.DownloadPath, FileMode.Create);
        await fileStream.CopyToAsync(outputFileStream);
    }

    public Version GetAppVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return new Version(fvi.FileVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0");
    }

    public void RunFinalCopyScript()
    {
        var scriptPath = "..\\restore.bat";
        var scriptFullPath = Path.GetFullPath(scriptPath);

        File.WriteAllText(scriptPath, Constants.Script.Finalize2);
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            WorkingDirectory = Path.GetDirectoryName(scriptFullPath),
            Arguments = "/c " + Path.GetFileName(scriptFullPath),
            UseShellExecute = false,
            CreateNoWindow = false, //
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process cmdProcess = Process.Start(psi);
        cmdProcess.Dispose();
    }
}