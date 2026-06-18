using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Unit;

[TestClass]
public sealed class CacheKeysTests
{
    [TestMethod]
    public void Trending_DifferentOffsets_DifferentKeys()
    {
        Assert.AreNotEqual(CacheKeys.Trending(0), CacheKeys.Trending(25));
    }

    [TestMethod]
    public void Search_DifferentTerms_DifferentKeys()
    {
        Assert.AreNotEqual(CacheKeys.Search("cats", 0), CacheKeys.Search("dogs", 0));
    }

    [TestMethod]
    public void Search_DifferentOffsets_DifferentKeys()
    {
        Assert.AreNotEqual(CacheKeys.Search("cats", 0), CacheKeys.Search("cats", 25));
    }

    [TestMethod]
    public void Search_AndTrending_DifferentKeys()
    {
        Assert.AreNotEqual(CacheKeys.Search("cats", 0), CacheKeys.Trending(0));
    }
}
