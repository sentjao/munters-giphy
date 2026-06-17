using Munters.Giphy.Api.Gifs;

namespace Munters.Giphy.Tests.Unit;

public sealed class QueryNormalizerTests
{
    [Theory]
    [InlineData("cats", "cats")]
    [InlineData("  cats  ", "cats")]
    [InlineData("CATS", "cats")]
    [InlineData("  FUNNY   CATS  ", "funny cats")]
    [InlineData("Funny Cats", "funny cats")]
    public void Normalize_ProducesExpectedKey(string input, string expected)
    {
        Assert.Equal(expected, QueryNormalizer.Normalize(input));
    }

    [Fact]
    public void Normalize_PreservesWordOrder()
    {
        Assert.Equal("cats dogs", QueryNormalizer.Normalize("Cats Dogs"));
        Assert.Equal("dogs cats", QueryNormalizer.Normalize("Dogs Cats"));
    }
}
