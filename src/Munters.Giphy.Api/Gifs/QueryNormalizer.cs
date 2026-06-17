using System.Text.RegularExpressions;

namespace Munters.Giphy.Api.Gifs;

internal static partial class QueryNormalizer
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();

    internal static string Normalize(string term) =>
        WhitespaceRun().Replace(term.Trim(), " ").ToLowerInvariant();
}
