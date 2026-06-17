namespace Munters.Giphy.Api.Gifs;

public interface IGifProvider
{
    Task<GifPage> GetTrendingAsync(int offset, CancellationToken ct);
    Task<GifPage> SearchAsync(string term, int offset, CancellationToken ct);

    // TODO: when country/region is exposed, add it to both the provider
    // calls and the cache key (e.g. "search:{country}:{term}:{offset}").
}
