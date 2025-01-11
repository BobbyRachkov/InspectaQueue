using Moq;
using Rachkov.InspectaQueue.Abstractions;
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
            _handler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_handler);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _sut = new AutoUpdaterService(httpClientFactoryMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
            _handler.Dispose();
        }

        [Theory]
        public async Task GivenCheckingForLatestVersion_WhenResponseIsIncorrectJson_ThenReturnsNull(ReleaseType releaseType)
        {
            //Arrange
            _handler.When("https://api.github.com/repos/BobbyRachkov/InspectaQueue/releases")
                .Respond("application/json", "{'name' : 'Test McGee'}");

            //Act
            var release = await _sut.GetLatestVersion(releaseType);

            //Assert
            Assert.That(release, Is.Null);
        }

        [TestCase(ReleaseType.Official, "0.1.2")]
        [TestCase(ReleaseType.Prerelease, "0.1.1")]
        public async Task GivenCheckingForLatestVersion_WhenResponseIsCorrectJson_ThenReturnsExpectedVersion(
            ReleaseType releaseType,
            string expectedVersion)
        {
            //Arrange
            _handler.When("https://api.github.com/repos/BobbyRachkov/InspectaQueue/releases")
                .Respond("application/json", TestData.ReleasesResponse);

            //Act
            var release = await _sut.GetLatestVersion(releaseType);

            //Assert
            Assert.That(release.Value.version.ToString(), Is.EqualTo(expectedVersion));
        }

        //[Ignore("Used for development purposes")]
        [Test]
        public async Task DownloadRealInfo()
        {
            _httpClient = new HttpClient();

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _sut = new AutoUpdaterService(httpClientFactoryMock.Object);
            var release = await _sut.GetLatestVersion(ReleaseType.Official);
        }
    }
}