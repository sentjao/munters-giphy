using System.Text.Json.Serialization;

namespace Munters.Giphy.Api.Giphy.Dtos;

internal sealed class GiphyListResponseDto
{
    [JsonPropertyName("data")]
    public IReadOnlyList<GiphyGifDto>? Data { get; init; }

    [JsonPropertyName("pagination")]
    public GiphyPaginationDto? Pagination { get; init; }
}
