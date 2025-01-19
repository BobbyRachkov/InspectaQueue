using Moq;
using Rachkov.InspectaQueue.AutoUpdater.Core;
using RichardSzalay.MockHttp;

namespace AutoUpdater.Tests
{
    public class Tests
    {
        private AutoUpdaterService _sut;
        private HttpClient _httpClient;
        private MockHttpMessageHandler _handler;

        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
            _handler.Dispose();
        }

        [Ignore("Used for development purposes")]
        [Test]
        public async Task DownloadRealInfo()
        {
            _httpClient = new HttpClient();

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            //_sut = new AutoUpdaterService(httpClientFactoryMock.Object);
            //var release = await _sut.GetLatestVersion(ReleaseType.Official);

            var downloader = new DownloadService(httpClientFactoryMock.Object);

            var info = await downloader.FetchReleaseInfoAsync();

            if (info?.Latest.WindowsAppZip is not null)
            {
                var result = await downloader.TryDownloadAssetAsync(info.Latest.WindowsAppZip, "kuramiqnko.zip");
            }
        }
    }
}