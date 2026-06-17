using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Unit;

public sealed class CacheKeysTests
{
    [Fact]
    public void Trending_DifferentOffsets_DifferentKeys()
    {
        Assert.NotEqual(CacheKeys.Trending(0), CacheKeys.Trending(25));
    }

    [Fact]
    public void Search_DifferentTerms_DifferentKeys()
    {
        Assert.NotEqual(CacheKeys.Search("cats", 0), CacheKeys.Search("dogs", 0));
    }

    [Fact]
    public void Search_DifferentOffsets_DifferentKeys()
    {
        Assert.NotEqual(CacheKeys.Search("cats", 0), CacheKeys.Search("cats", 25));
    }

    [Fact]
    public void Search_AndTrending_DifferentKeys()
    {
        Assert.NotEqual(CacheKeys.Search("cats", 0), CacheKeys.Trending(0));
    }
}
