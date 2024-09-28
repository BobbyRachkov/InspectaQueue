using Rachkov.InspectaQueue.Abstractions.Contracts;

namespace Rachkov.InspectaQueue.Abstractions.Extensions;

internal static class ReleaseContractExtensions
{
    public static string? GetBrowserDownloadString(this Release? release)
    {
        return release?.Assets.FirstOrDefault()?.BrowserDownloadUrl;
    }
}