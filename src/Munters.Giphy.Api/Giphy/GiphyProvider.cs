using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy.Dtos;

namespace Munters.Giphy.Api.Giphy;

internal sealed class GiphyProvider : IGifProvider
{
    private readonly HttpClient _http;
    private readonly GiphyOptions _opts;
    private readonly ILogger<GiphyProvider> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public GiphyProvider(HttpClient http, IOptions<GiphyOptions> opts, ILogger<GiphyProvider> logger)
    {
        _http = http;
        _opts = opts.Value;
        _logger = logger;
    }

    public Task<GifPage> GetTrendingAsync(int offset, CancellationToken ct) =>
        FetchAsync($"v1/gifs/trending?api_key={_opts.ApiKey}&limit={_opts.PageSize}&offset={offset}", offset, ct);

    public Task<GifPage> SearchAsync(string term, int offset, CancellationToken ct) =>
        FetchAsync($"v1/gifs/search?api_key={_opts.ApiKey}&q={Uri.EscapeDataString(term)}&limit={_opts.PageSize}&offset={offset}", offset, ct);

    private async Task<GifPage> FetchAsync(string relativeUrl, int offset, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_opts.RequestTimeout);

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(relativeUrl, cts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("GIPHY request timed out after {Timeout}", _opts.RequestTimeout);
            throw new GiphyUpstreamTimeoutException("GIPHY request timed out.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GIPHY HTTP request failed");
            throw new GiphyUpstreamException("GIPHY request failed.", ex);
        }

        ValidateSuccessfulResponse(response);

        var dto = await DeserializeResponseAsync(response, ct);
        return MapPage(dto, offset);
    }

    private async Task<GiphyListResponseDto?> DeserializeResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<GiphyListResponseDto>(JsonOpts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize GIPHY response");
            throw new GiphyUpstreamException("Invalid response from GIPHY.", ex);
        }
    }

    private void ValidateSuccessfulResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GIPHY returned {StatusCode}", (int)response.StatusCode);
            throw new GiphyUpstreamException($"GIPHY returned {(int)response.StatusCode}.");
        }
    }

    private GifPage MapPage(GiphyListResponseDto? dto, int offset)
    {
        if (dto?.Data is null)
            return new GifPage([], offset, _opts.PageSize, 0, null);

        var items = dto.Data
            .Where(g => !string.IsNullOrWhiteSpace(g.Id) && !string.IsNullOrWhiteSpace(g.Images?.Original?.Url))
            .Select(g => new GifItem(g.Id!, g.Images!.Original!.Url!))
            .ToList();    

        return new GifPage(
            items,
            dto.Pagination?.Offset ?? offset,
            _opts.PageSize,
            items.Count,
            dto.Pagination?.TotalCount);
    }
}
