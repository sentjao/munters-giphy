namespace Munters.Giphy.Api.Gifs;

internal static class CacheKeys
{
    internal static string Trending(int offset) => $"trending:{offset}";
    internal static string Search(string normalizedTerm, int offset) => $"search:{normalizedTerm}:{offset}";
}
