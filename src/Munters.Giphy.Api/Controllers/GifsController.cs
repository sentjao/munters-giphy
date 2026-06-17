using Microsoft.AspNetCore.Mvc;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

namespace Munters.Giphy.Api.Controllers;

[ApiController]
[Route("api/gifs")]
[Produces("application/json")]
public sealed class GifsController : ControllerBase
{
    private readonly GifCatalogService _catalog;
    private readonly ILogger<GifsController> _logger;

    public GifsController(GifCatalogService catalog, ILogger<GifsController> logger)
    {
        _catalog = catalog;
        _logger = logger;
    }

    /// <summary>Returns trending GIFs for a given page offset.</summary>
    [HttpGet("trending")]
    [ProducesResponseType(typeof(GifPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetTrending([FromQuery] int offset = 0, CancellationToken ct = default)
    {
        if (offset < 0)
            return ValidationProblem("offset must be >= 0", paramName: "offset");

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Trending",
            ["RequestId"] = HttpContext.TraceIdentifier,
            ["Offset"] = offset
        }))
        {
            var page = await _catalog.GetTrendingAsync(offset, ct);
            return Ok(ToResponse(page));
        }
    }

    /// <summary>Returns GIFs matching the given search term.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(GifPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> Search([FromQuery] string? term, [FromQuery] int offset = 0, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term))
            return ValidationProblem("term is required", paramName: "term");

        if (offset < 0)
            return ValidationProblem("offset must be >= 0", paramName: "offset");

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "Search",
            ["RequestId"] = HttpContext.TraceIdentifier,
            ["Offset"] = offset
        }))
        {
            var page = await _catalog.SearchAsync(term, offset, ct);
            return Ok(ToResponse(page));
        }
    }

    private static GifPageResponse ToResponse(GifPage page) =>
        new(page.Items.Select(i => i.Url).ToList(), page.Offset, page.PageSize, page.Count, page.TotalCount);

    private IActionResult ValidationProblem(string detail, string paramName)
    {
        var problem = new ValidationProblemDetails
        {
            Detail = detail,
            Status = StatusCodes.Status400BadRequest
        };
        problem.Errors[paramName] = [detail];
        return BadRequest(problem);
    }
}
