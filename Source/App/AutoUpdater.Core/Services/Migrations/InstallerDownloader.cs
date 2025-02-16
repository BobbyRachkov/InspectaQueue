using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public class InstallerDownloader : IDisposable, IInstallerDownloader
{
    private readonly HttpClient _httpClient = new();
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    public async Task<bool> DownloadInstaller(string url, AbsolutePath downloadPath, CancellationToken cancellationToken = default)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if (attempt == MaxRetries) return false;
                    await Task.Delay(RetryDelayMs, cancellationToken);
                    continue;
                }

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = File.Create(downloadPath);
                await contentStream.CopyToAsync(fileStream, cancellationToken);

                return true;
            }
            catch
            {
                downloadPath.DeleteFile();
                if (attempt == MaxRetries) return false;
                await Task.Delay(RetryDelayMs, cancellationToken);
            }
        }

        return false;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}