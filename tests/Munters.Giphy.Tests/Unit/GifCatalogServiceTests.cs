using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Unit;

[TestClass]
public sealed class GifCatalogServiceTests
{
    private ServiceProvider _sp = null!;
    private Mock<IGifProvider> _providerMock = null!;
    private GifCatalogService _service = null!;

    private static GifPage DefaultPage(int offset) => new(
        [new GifItem("id1", "https://media.giphy.com/test.gif")],
        offset, 25, 1, 100);

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddLogging();
        _sp = services.BuildServiceProvider();

        _providerMock = new Mock<IGifProvider>();
        _providerMock
            .Setup(p => p.GetTrendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int offset, CancellationToken ct) => DefaultPage(offset));
        _providerMock
            .Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string term, int offset, CancellationToken ct) => DefaultPage(offset));

        var opts = Options.Create(new GiphyOptions
        {
            CacheDuration = TimeSpan.FromMilliseconds(500),
            PageSize = 25,
            BaseUrl = "https://api.giphy.com",
            ApiKey = "test"
        });

        _service = new GifCatalogService(
            _providerMock.Object,
            _sp.GetRequiredService<HybridCache>(),
            opts);
    }

    [TestCleanup]
    public void Cleanup() => _sp.Dispose();

    [TestMethod]
    public async Task Search_CacheHit_ProviderCalledOnce()
    {
        await _service.SearchAsync("cats", 0, CancellationToken.None);
        await _service.SearchAsync("cats", 0, CancellationToken.None);

        _providerMock.Verify(
            p => p.SearchAsync("cats", 0, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Search_AfterTtlExpires_CallsProviderAgain()
    {
        await _service.SearchAsync("cats", 0, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1)); // TTL is 500ms, so cache is expired
        await _service.SearchAsync("cats", 0, CancellationToken.None);

        _providerMock.Verify(
            p => p.SearchAsync("cats", 0, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [TestMethod]
    public async Task Trending_CacheHit_ProviderCalledOnce()
    {
        await _service.GetTrendingAsync(0, CancellationToken.None);
        await _service.GetTrendingAsync(0, CancellationToken.None);

        _providerMock.Verify(
            p => p.GetTrendingAsync(0, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Trending_AfterTtlExpires_CallsProviderAgain()
    {
        await _service.GetTrendingAsync(0, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1)); // TTL is 500ms, so cache is expired
        await _service.GetTrendingAsync(0, CancellationToken.None);

        _providerMock.Verify(
            p => p.GetTrendingAsync(0, It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
