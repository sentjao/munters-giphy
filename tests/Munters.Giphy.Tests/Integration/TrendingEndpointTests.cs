using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Integration;

[TestClass]
public sealed class TrendingEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<IGifProvider> _providerMock = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        (_factory, _providerMock) = TestApp.Create();
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task GetTrending_Returns200WithUrls()
    {
        var response = await _client.GetAsync("/api/gifs/trending");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GifPageResponse>();
        Assert.IsNotNull(body);
        Assert.IsTrue(body.Urls.Count > 0);
        Assert.AreEqual("https://media.giphy.com/test.gif", body.Urls[0]);
    }

    [TestMethod]
    public async Task GetTrending_NegativeOffset_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/trending?offset=-1");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTrending_CacheHit_ProviderCalledOnce()
    {
        _ = await _client.GetAsync("/api/gifs/trending?offset=999");
        _ = await _client.GetAsync("/api/gifs/trending?offset=999");

        _providerMock.Verify(
            p => p.GetTrendingAsync(999, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task GetTrending_UpstreamTimeout_Returns504()
    {
        _providerMock
            .Setup(p => p.GetTrendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GiphyUpstreamTimeoutException("timeout"));

        var response = await _client.GetAsync("/api/gifs/trending");
        Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTrending_UpstreamError_Returns502()
    {
        _providerMock
            .Setup(p => p.GetTrendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GiphyUpstreamException("bad gateway"));

        var response = await _client.GetAsync("/api/gifs/trending");
        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTrending_ResponseDoesNotExposeGiphyDtos()
    {
        var response = await _client.GetAsync("/api/gifs/trending");
        var json = await response.Content.ReadAsStringAsync();

        Assert.IsFalse(json.Contains("\"data\""));
        Assert.IsFalse(json.Contains("\"pagination\""));
        Assert.IsFalse(json.Contains("\"images\""));
    }
}
