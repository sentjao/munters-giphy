using System.Net;
using System.Net.Http.Json;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Integration;

public sealed class TrendingEndpointTests : IClassFixture<GifsApiFactory>
{
    private readonly GifsApiFactory _factory;
    private readonly HttpClient _client;

    public TrendingEndpointTests(GifsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTrending_Returns200WithUrls()
    {
        var response = await _client.GetAsync("/api/gifs/trending");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GifPageResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Urls);
        Assert.Equal("https://media.giphy.com/test.gif", body.Urls[0]);
    }

    [Fact]
    public async Task GetTrending_NegativeOffset_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/trending?offset=-1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTrending_CacheHit_ProviderCalledOnce()
    {
        using var factory = new GifsApiFactory();
        var client = factory.CreateClient();

        _ = await client.GetAsync("/api/gifs/trending?offset=999");
        _ = await client.GetAsync("/api/gifs/trending?offset=999");

        Assert.Equal(1, factory.FakeProvider.TrendingCallCount);
    }

    [Fact]
    public async Task GetTrending_UpstreamTimeout_Returns504()
    {
        using var factory = new GifsApiFactory();
        factory.FakeProvider.ThrowOnTrending = new GiphyUpstreamTimeoutException("timeout");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/gifs/trending");
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [Fact]
    public async Task GetTrending_UpstreamError_Returns502()
    {
        using var factory = new GifsApiFactory();
        factory.FakeProvider.ThrowOnTrending = new GiphyUpstreamException("bad gateway");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/gifs/trending");
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task GetTrending_ResponseDoesNotExposeGiphyDtos()
    {
        var response = await _client.GetAsync("/api/gifs/trending");
        var json = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("data", json);
        Assert.DoesNotContain("pagination", json);
        Assert.DoesNotContain("images", json);
    }
}
