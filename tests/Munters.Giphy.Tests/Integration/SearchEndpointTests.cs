using System.Net;
using System.Net.Http.Json;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Integration;

public sealed class SearchEndpointTests : IClassFixture<GifsApiFactory>
{
    private readonly GifsApiFactory _factory;
    private readonly HttpClient _client;

    public SearchEndpointTests(GifsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_Returns200WithUrls()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=cats");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GifPageResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body.Urls);
    }

    [Fact]
    public async Task Search_MissingTerm_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_EmptyTerm_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=   ");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_NegativeOffset_Returns400()
    {
        var response = await _client.GetAsync("/api/gifs/search?term=cats&offset=-1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_CacheHit_ProviderCalledOnce()
    {
        using var factory = new GifsApiFactory();
        var client = factory.CreateClient();

        _ = await client.GetAsync("/api/gifs/search?term=uniqueterm9182");
        _ = await client.GetAsync("/api/gifs/search?term=uniqueterm9182");

        Assert.Equal(1, factory.FakeProvider.SearchCallCount);
    }

    [Fact]
    public async Task Search_NormalizationCollapses_ToSameKey()
    {
        using var factory = new GifsApiFactory();
        var client = factory.CreateClient();

        _ = await client.GetAsync("/api/gifs/search?term=FUNKY+CATS");
        _ = await client.GetAsync("/api/gifs/search?term=funky+cats");

        Assert.Equal(1, factory.FakeProvider.SearchCallCount);
    }

    [Fact]
    public async Task Search_DifferentTerms_DifferentCacheEntries()
    {
        using var factory = new GifsApiFactory();
        var client = factory.CreateClient();

        _ = await client.GetAsync("/api/gifs/search?term=cats9999");
        _ = await client.GetAsync("/api/gifs/search?term=dogs9999");

        Assert.Equal(2, factory.FakeProvider.SearchCallCount);
    }

    [Fact]
    public async Task Search_UpstreamTimeout_Returns504()
    {
        using var factory = new GifsApiFactory();
        factory.FakeProvider.ThrowOnSearch = new GiphyUpstreamTimeoutException("timeout");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/gifs/search?term=cats");
        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [Fact]
    public async Task Search_UpstreamError_Returns502()
    {
        using var factory = new GifsApiFactory();
        factory.FakeProvider.ThrowOnSearch = new GiphyUpstreamException("bad gateway");
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/gifs/search?term=cats");
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Search_FailureNotCached_SecondCallRetries()
    {
        using var factory = new GifsApiFactory();
        var client = factory.CreateClient();

        factory.FakeProvider.ThrowOnSearch = new GiphyUpstreamException("error");
        _ = await client.GetAsync("/api/gifs/search?term=retryterm1234");

        factory.FakeProvider.ThrowOnSearch = null;
        var response = await client.GetAsync("/api/gifs/search?term=retryterm1234");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, factory.FakeProvider.SearchCallCount);
    }
}
