using Newtonsoft.Json;
using Rachkov.InspectaQueue.Abstractions.Contracts;
using Rachkov.InspectaQueue.Abstractions.Extensions;
using System.Net.Http;
using System.Net.Http.Headers;

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

            var prerelease = allReleases?.FirstOrDefault(x => x.Prerelease is true);

            if (prerelease?.TagName is null)
            {
                return null;
            }

            return (new Version(prerelease.TagName), prerelease.GetBrowserDownloadString());
        }
        catch(Exception e)
        {
            return null;
        }
    }

    public async Task DownloadVersion(string downloadUrl, string outputFileLocation)
    {
        var client = _httpClientFactory.CreateClient();
        Stream fileStream = await client.GetStreamAsync(downloadUrl);

        await using FileStream outputFileStream = new FileStream(outputFileLocation, FileMode.Create);
        await fileStream.CopyToAsync(outputFileStream);
    }
}