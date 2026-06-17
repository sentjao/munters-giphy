using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Tests.Integration;

/// <summary>
/// A deterministic, controllable IGifProvider for integration tests.
/// </summary>
public sealed class FakeGifProvider : IGifProvider
{
    public int TrendingCallCount { get; private set; }
    public int SearchCallCount { get; private set; }

    public Func<int, GifPage>? TrendingFactory { get; set; }
    public Func<string, int, GifPage>? SearchFactory { get; set; }

    public Exception? ThrowOnTrending { get; set; }
    public Exception? ThrowOnSearch { get; set; }

    private static GifPage DefaultPage(int offset) => new(
        [new GifItem("id1", "https://media.giphy.com/test.gif")],
        offset, 25, 1, 100);

    public Task<GifPage> GetTrendingAsync(int offset, CancellationToken ct)
    {
        TrendingCallCount++;
        if (ThrowOnTrending is not null) throw ThrowOnTrending;
        var page = TrendingFactory?.Invoke(offset) ?? DefaultPage(offset);
        return Task.FromResult(page);
    }

    public Task<GifPage> SearchAsync(string term, int offset, CancellationToken ct)
    {
        SearchCallCount++;
        if (ThrowOnSearch is not null) throw ThrowOnSearch;
        var page = SearchFactory?.Invoke(term, offset) ?? DefaultPage(offset);
        return Task.FromResult(page);
    }
}
