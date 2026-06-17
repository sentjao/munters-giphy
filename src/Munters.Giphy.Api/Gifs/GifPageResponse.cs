namespace Munters.Giphy.Api.Gifs;

public sealed record GifPageResponse(
    IReadOnlyList<string> Urls,
    int Offset,
    int PageSize,
    int Count,
    int? TotalCount);
