using System.Net.Http;

namespace AutoUpdater.App.Services;

public class HttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient();
    }
}