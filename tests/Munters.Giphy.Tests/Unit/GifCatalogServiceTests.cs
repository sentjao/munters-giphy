using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;
using Munters.Giphy.Tests.Integration;

namespace Munters.Giphy.Tests.Unit;

public sealed class GifCatalogServiceTests : IDisposable
{
    private readonly ServiceProvider _sp;
    private readonly FakeGifProvider _provider;
    private readonly GifCatalogService _service;

    public GifCatalogServiceTests()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddLogging();
        _sp = services.BuildServiceProvider();

        _provider = new FakeGifProvider();

        var opts = Options.Create(new GiphyOptions
        {
            CacheDuration = TimeSpan.FromMilliseconds(500),
            PageSize = 25,
            BaseUrl = "https://api.giphy.com",
            ApiKey = "test"
        });

        _service = new GifCatalogService(_provider, _sp.GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(), opts);
    }

    public void Dispose() => _sp.Dispose();

    [Fact]
    public async Task Search_CacheHit_ProviderCalledOnce()
    {
        await _service.SearchAsync("cats", 0, CancellationToken.None);
        await _service.SearchAsync("cats", 0, CancellationToken.None);

        Assert.Equal(1, _provider.SearchCallCount);
    }

    [Fact]
    public async Task Search_AfterTtlExpires_CallsProviderAgain()
    {
        await _service.SearchAsync("cats", 0, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1)); // TTL is 500ms, so cache is expired
        await _service.SearchAsync("cats", 0, CancellationToken.None);

        Assert.Equal(2, _provider.SearchCallCount);
    }

    [Fact]
    public async Task Trending_CacheHit_ProviderCalledOnce()
    {
        await _service.GetTrendingAsync(0, CancellationToken.None);
        await _service.GetTrendingAsync(0, CancellationToken.None);

        Assert.Equal(1, _provider.TrendingCallCount);
    }

    [Fact]
    public async Task Trending_AfterTtlExpires_CallsProviderAgain()
    {
        await _service.GetTrendingAsync(0, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1)); // TTL is 500ms, so cache is expired
        await _service.GetTrendingAsync(0, CancellationToken.None);

        Assert.Equal(2, _provider.TrendingCallCount);
    }
}
