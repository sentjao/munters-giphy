#pragma warning disable EXTEXP0018 // HybridCache is GA and production-ready on .NET 10

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Munters.Giphy.Api.Gifs;
using Munters.Giphy.Api.Giphy;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Services
    .AddOptions<GiphyOptions>()
    .BindConfiguration(GiphyOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Typed HttpClient for GIPHY
builder.Services
    .AddHttpClient<IGifProvider, GiphyProvider>((sp, client) =>
    {
        var opts = sp.GetRequiredService<IOptions<GiphyOptions>>().Value;
        client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
    });

// Application services
builder.Services.AddSingleton<GifCatalogService>();

// HybridCache (L1 in-memory only — no IDistributedCache registered)
builder.Services.AddHybridCache(o =>
{
    o.MaximumKeyLength = 512;
    o.MaximumPayloadBytes = 512 * 1024; // 512 KB per entry
    o.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Flags = HybridCacheEntryFlags.DisableDistributedCache
    };
});

// Controllers, Swagger, ProblemDetails
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Munters GIPHY API", Version = "v1" });
});
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// CORS for local React dev
builder.Services.AddCors(o =>
    o.AddPolicy("LocalDev", p => p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler(err =>
{
    err.Run(async ctx =>
    {
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        var exFeature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = exFeature?.Error;

        if (ex is GiphyUpstreamTimeoutException)
        {
            ctx.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            logger.LogWarning("GIPHY request timed out");
        }
        else if (ex is GiphyUpstreamException)
        {
            ctx.Response.StatusCode = StatusCodes.Status502BadGateway;
            logger.LogWarning("GIPHY service error");
        }
        else if (ex is OperationCanceledException)
        {
            ctx.Response.StatusCode = 499;
            return;
        }
        else
        {
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            logger.LogError(ex, "Unhandled exception");
        }

        var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = ctx.Response.StatusCode,
            Title = ctx.Response.StatusCode switch
            {
                504 => "Upstream request timed out",
                502 => "Upstream service error",
                _ => "An unexpected error occurred"
            }
        };
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(pd);
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("LocalDev");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Expose Program for WebApplicationFactory in tests
public partial class Program { }
