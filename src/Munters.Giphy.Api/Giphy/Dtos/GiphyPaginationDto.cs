using System.Text.Json.Serialization;

namespace Munters.Giphy.Api.Giphy.Dtos;

internal sealed class GiphyPaginationDto
{
    [JsonPropertyName("total_count")]
    public int? TotalCount { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("offset")]
    public int Offset { get; init; }
}
