﻿using Newtonsoft.Json;
using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Core.Models;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Download;

public sealed class DownloadService : IDownloadService
{
    private readonly HttpClient _client;

    public DownloadService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri(Constants.Url.RepositoryApi);
        _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product", "1"));
    }

    public async Task<ReleaseInfo?> FetchReleaseInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allReleasesJson = await _client.GetStringAsync(Constants.Url.ReleasesPath, cancellationToken);
            var allReleases = JsonConvert.DeserializeObject<Contracts.Release[]>(allReleasesJson);

            var latest = allReleases?.FirstOrDefault(x => x.Prerelease is false);
            var prerelease = allReleases?.FirstOrDefault();

            Release? latestModel = null, prereleaseModel = null;

            if (latest is not null)
            {
                latestModel = new Release
                {
                    Name = latest.Name ?? "unnamed",
                    Tag = latest.TagName ?? string.Empty,
                    IsLatest = true,
                    IsPrerelease = false,
                    Assets = ParseAssets(latest),
                    WindowsAppZip = GetWindowsZip(latest),
                    Installer = GetInstaller(latest)
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
                    Assets = ParseAssets(prerelease),
                    WindowsAppZip = GetWindowsZip(prerelease),
                    Installer = GetInstaller(prerelease)
                };
            }

            return latestModel is null || prereleaseModel is null
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

        if (versionGroups?.Groups.Count != 4)
        {
            return null;
        }

        var major = int.Parse(versionGroups.Groups[1].Value);
        var minor = int.Parse(versionGroups.Groups[2].Value);
        var patch = int.Parse(versionGroups.Groups[3].Value);

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

    public async Task<bool> TryDownloadAssetAsync(Asset asset, AbsolutePath downloadPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(asset.DownloadUrl))
        {
            return false;
        }

        try
        {
            var fileStream = await _client.GetStreamAsync(asset.DownloadUrl, cancellationToken);

            downloadPath.DeleteFile();
            await using var outputFileStream = new FileStream(downloadPath, FileMode.Create);
            await fileStream.CopyToAsync(outputFileStream, cancellationToken);
            outputFileStream.Close();
            fileStream.Close();

            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}