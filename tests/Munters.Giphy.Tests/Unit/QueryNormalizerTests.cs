using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Unit;

[TestClass]
public sealed class QueryNormalizerTests
{
    [DataTestMethod]
    [DataRow("cats", "cats")]
    [DataRow("  cats  ", "cats")]
    [DataRow("CATS", "cats")]
    [DataRow("  FUNNY   CATS  ", "funny cats")]
    [DataRow("Funny Cats", "funny cats")]
    public void Normalize_ProducesExpectedKey(string input, string expected)
    {
        Assert.AreEqual(expected, QueryNormalizer.Normalize(input));
    }

    [TestMethod]
    public void Normalize_PreservesWordOrder()
    {
        Assert.AreEqual("cats dogs", QueryNormalizer.Normalize("Cats Dogs"));
        Assert.AreEqual("dogs cats", QueryNormalizer.Normalize("Dogs Cats"));
    }
}
