using System.Text.Json.Serialization;

namespace Munters.Giphy.Api.Giphy.Dtos;

internal sealed class GiphyGifDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    //The nesting mimics the structure of GIPHY's JSON response, which has the URL inside "images/original" -> "original".
    [JsonPropertyName("images")]
    public GiphyImagesDto? Images { get; init; }
}

internal sealed class GiphyImagesDto
{
    [JsonPropertyName("original")]
    public GiphyRenditionDto? Original { get; init; }
}

internal sealed class GiphyRenditionDto
{
    [JsonPropertyName("url")]
    public string? Url { get; init; }
}
