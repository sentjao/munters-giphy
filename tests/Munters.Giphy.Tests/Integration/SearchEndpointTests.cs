using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Integration;

[TestClass]
public sealed class SearchEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<IGifProvider> _providerMock = null!;
    private HttpClient _client = null!;

    private static GifPage DefaultPage(int offset) => new(
        [new GifItem("id1", "https://media.giphy.com/test.gif")],
        offset, 25, 1, 100);

    [TestInitialize]
    public void Setup()
    {
        (_factory, _providerMock) = TestApp.Create();
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task Search_Returns200WithUrls()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=cats");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GifPageResponse>();
        Assert.IsNotNull(body);
        Assert.IsTrue(body.Urls.Count > 0);
    }

    [TestMethod]
    public async Task Search_MissingTerm_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Search_EmptyTerm_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=   ");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Search_NegativeOffset_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=cats&offset=-1");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Search_CacheHit_ProviderCalledOnce()
    {
        _ = await _client.GetAsync("/api/gifs/search?term=uniqueterm9182");
        _ = await _client.GetAsync("/api/gifs/search?term=uniqueterm9182");

        _providerMock.Verify(
            p => p.SearchAsync("uniqueterm9182", 0, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Search_NormalizationCollapses_ToSameKey()
    {
        _ = await _client.GetAsync("/api/gifs/search?term=FUNKY+CATS");
        _ = await _client.GetAsync("/api/gifs/search?term=funky+cats");

        _providerMock.Verify(
            p => p.SearchAsync("funky cats", 0, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Search_DifferentTerms_DifferentCacheEntries()
    {
        _ = await _client.GetAsync("/api/gifs/search?term=cats9999");
        _ = await _client.GetAsync("/api/gifs/search?term=dogs9999");

        _providerMock.Verify(
            p => p.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [TestMethod]
    public async Task Search_UpstreamTimeout_Returns504()
    {
        _providerMock
            .Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GiphyUpstreamTimeoutException("timeout"));

        var response = await _client.GetAsync("/api/gifs/search?term=cats");
        Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [TestMethod]
    public async Task Search_UpstreamError_Returns502()
    {
        _providerMock
            .Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GiphyUpstreamException("bad gateway"));

        var response = await _client.GetAsync("/api/gifs/search?term=cats");
        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [TestMethod]
    public async Task Search_FailureNotCached_SecondCallRetries()
    {
        _providerMock
            .SetupSequence(p => p.SearchAsync("retryterm1234", 0, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GiphyUpstreamException("error"))
            .ReturnsAsync(DefaultPage(0));

        _ = await _client.GetAsync("/api/gifs/search?term=retryterm1234");
        var response = await _client.GetAsync("/api/gifs/search?term=retryterm1234");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _providerMock.Verify(
            p => p.SearchAsync("retryterm1234", 0, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
