using Newtonsoft.Json;
using Rachkov.InspectaQueue.Abstractions.Models;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Rachkov.InspectaQueue.Abstractions;

public sealed class DownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DownloadService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ReleaseInfo?> FetchReleaseInfoAsync()
    {
        try
        {
            var allReleasesJson = await CreateClient().GetStringAsync(Constants.Url.ReleasesPath);
            var allReleases = JsonConvert.DeserializeObject<Contracts.Release[]>(allReleasesJson);

            var latest = allReleases?.FirstOrDefault(x => x.Prerelease is false);
            var prerelease = allReleases?.FirstOrDefault(x => x.Prerelease is true);

            Release? latestModel = null, prereleaseModel = null;

            if (latest is not null)
            {
                latestModel = new Release
                {
                    Name = latest.Name ?? "unnamed",
                    Tag = latest.TagName ?? string.Empty,
                    IsLatest = true,
                    IsPrerelease = false,
                    Assets = ParseAssets(latest)
                };
            }

            if (prerelease is not null)
            {
                prereleaseModel = new Release
                {
                    Name = prerelease.Name ?? "unnamed",
                    Tag = prerelease.TagName ?? string.Empty,
                    IsLatest = false,
                    IsPrerelease = true,
                    Assets = ParseAssets(prerelease)
                };
            }

            return latestModel is null
                ? null
                : new ReleaseInfo { Latest = latestModel, Prerelease = prereleaseModel };
        }
        catch
        {
            return null;
        }
    }

    private static Asset[] ParseAssets(Contracts.Release release)
    {
        return release.Assets.Select(ParseAsset).ToArray();
    }

    private static Asset ParseAsset(Contracts.Asset asset)
    {
        return new Asset
        {
            Name = asset.Name ?? string.Empty,
            DownloadCount = asset.DownloadCount ?? -1,
            Url = asset.Url ?? string.Empty,
            DownloadUrl = asset.BrowserDownloadUrl,
            Version = ParseVersion(asset.Name ?? string.Empty)
        };
    }

    private static Version? ParseVersion(string assetName)
    {
        string pattern = @"_(\d+)\.(\d+)\.(\d+)\.";

        var versionGroups = Regex.Matches(assetName, pattern, RegexOptions.Multiline).FirstOrDefault();

        if (versionGroups?.Groups.Count != 3)
        {
            return null;
        }

        var major = int.Parse(versionGroups.Groups[0].Value);
        var minor = int.Parse(versionGroups.Groups[0].Value);
        var patch = int.Parse(versionGroups.Groups[0].Value);

        return new Version(major, minor, patch);
    }

    private static Asset? GetWindowsZip(Contracts.Release release)
    {
        var asset = release.Assets.FirstOrDefault(x => x.Name is not null && x.Name.StartsWith("InspectaQueue") && x.Name.EndsWith(".zip"));
        return asset is null ? null : ParseAsset(asset);
    }

    private static Asset? GetInstaller(Contracts.Release release)
    {
        var asset = release.Assets.FirstOrDefault(x => x.Name is not null && x.Name.StartsWith("Installer") && x.Name.EndsWith(".exe"));
        return asset is null ? null : ParseAsset(asset);
    }


    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(Constants.Url.RepositoryApi);
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product", "1"));
        return client;
    }
}