namespace Munters.Giphy.Api.Gifs;

public sealed record GifPage(
    IReadOnlyList<GifItem> Items,
    int Offset,
    int PageSize,
    int Count,
    int? TotalCount);
