#pragma warning disable EXTEXP0018 // HybridCache is GA and production-ready on .NET 10

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Api.Gifs;

public sealed class GifCatalogService
{
    private readonly IGifProvider _provider;
    private readonly HybridCache _cache;
    private readonly HybridCacheEntryOptions _cacheOptions;

    public GifCatalogService(IGifProvider provider, HybridCache cache, IOptions<GiphyOptions> opts)
    {
        _provider = provider;
        _cache = cache;
        _cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = opts.Value.CacheDuration,
            LocalCacheExpiration = opts.Value.CacheDuration
        };
    }

    public async Task<GifPage> GetTrendingAsync(int offset, CancellationToken ct) =>
        await _cache.GetOrCreateAsync(
            CacheKeys.Trending(offset),
            async innerCt => await _provider.GetTrendingAsync(offset, innerCt),
            _cacheOptions,
            cancellationToken: ct);

    public async Task<GifPage> SearchAsync(string rawTerm, int offset, CancellationToken ct)
    {
        var term = QueryNormalizer.Normalize(rawTerm);
        return await _cache.GetOrCreateAsync(
            CacheKeys.Search(term, offset),
            async innerCt => await _provider.SearchAsync(term, offset, innerCt),
            _cacheOptions,
            cancellationToken: ct);
    }
}
