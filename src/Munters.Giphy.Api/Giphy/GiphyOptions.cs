using System.ComponentModel.DataAnnotations;

namespace Munters.Giphy.Api.Giphy;

public sealed class GiphyOptions
{
    public const string SectionName = "Giphy";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(10);

    [Range(1, 100)]
    public int PageSize { get; init; } = 25;

    [Range(typeof(TimeSpan), "00:00:01", "24:00:00")]
    public TimeSpan CacheDuration { get; init; } = TimeSpan.FromMinutes(10);
}
