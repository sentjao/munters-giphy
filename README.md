# Munters GIPHY API

A small ASP.NET Core 10 application that exposes two endpoints over the GIPHY public API: **Trending** and **Search**. It demonstrates a clean single-deployable architecture with a polymorphic provider seam, lazy HybridCache caching, and stampede protection.

## Architecture

```
GifsController  (validation, ProblemDetails, Swagger)
      |
GifCatalogService  (Trending + Search use cases)
      |
      +-------------------+
      |                   |
 HybridCache (L1)    IGifProvider  ← swappable seam
 lazy fill           GiphyProvider (typed HttpClient)
 stampede guard            |
                      GIPHY REST API
```

**Key design decisions**

| Area | Decision |
|---|---|
| Deployment | Single ASP.NET Core project; modules are namespaces, not assemblies. |
| Caching | HybridCache L1-only — lazy `GetOrCreateAsync`, no background worker. |
| Stampede protection | Framework: concurrent misses for the same key coalesce to one upstream call. |
| Provider seam | `IGifProvider` is the only polymorphic point; GIPHY DTOs stay internal. |
| Cache TTL | Single absolute TTL for all entries (HybridCache fixes options before the result is known). |
| Failures | Throwing factory propagates and is not cached; next request retries normally. |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A [GIPHY API key](https://developers.giphy.com/) (free beta key is sufficient)

## Setup

**1. Restore and build**

```bash
dotnet build -c Release
```

**2. Set your GIPHY API key (never committed to the repository)**

```bash
dotnet user-secrets init --project src/Munters.Giphy.Api
dotnet user-secrets set "Giphy:ApiKey" "<YOUR_KEY>" --project src/Munters.Giphy.Api
```

Alternatively, set the environment variable:

```bash
# Windows PowerShell
$env:Giphy__ApiKey = "<YOUR_KEY>"

# Linux / macOS
export Giphy__ApiKey="<YOUR_KEY>"
```

## Run

```bash
dotnet run --project src/Munters.Giphy.Api
```

Swagger UI: `https://localhost:<port>/swagger`
Health check: `https://localhost:<port>/health`

## Test

```bash
dotnet test
```

All tests run in-memory via `WebApplicationFactory` and require no network access or GIPHY key.

## Endpoints

| Method | Route | Parameters | Description |
|---|---|---|---|
| GET | `/api/gifs/trending` | `offset` (default 0) | Returns a page of trending GIF URLs. |
| GET | `/api/gifs/search` | `term` (required), `offset` (default 0) | Returns a page of GIF URLs matching the search term. |
| GET | `/health` | — | Lightweight liveness check. |

### Response shape

```json
{
  "urls": ["https://media.giphy.com/media/.../giphy.gif"],
  "offset": 0,
  "pageSize": 25,
  "count": 25,
  "totalCount": 250
}
```

`totalCount` reflects the GIPHY-reported pagination total for the requested query.

### Error responses

| Condition | Status |
|---|---|
| Missing/empty term, negative offset | 400 Bad Request |
| GIPHY returned non-2xx | 502 Bad Gateway |
| GIPHY request timed out | 504 Gateway Timeout |
| Unexpected error | 500 Internal Server Error |

## Configuration

All settings live under the `Giphy` key in `appsettings.json`. Override any value via environment variables (`Giphy__PageSize=10`) or user secrets.

| Setting | Default | Description |
|---|---|---|
| `BaseUrl` | `https://api.giphy.com` | GIPHY API base address |
| `ApiKey` | *(required — use secrets)* | GIPHY API credential |
| `RequestTimeout` | `00:00:10` | Upstream HTTP timeout |
| `PageSize` | `25` | Fixed page size for all requests |
| `CacheDuration` | `00:10:00` | Absolute TTL for cached pages |

## Cache behaviour

- Both Trending and Search use **lazy** caching: a cache miss triggers one upstream call, stores the result, and serves all subsequent identical requests from cache until the TTL expires.
- **Stampede protection**: concurrent requests for the same uncached key coalesce to a single upstream call (HybridCache `GetOrCreateAsync`).
- **Failures are never cached**: a throwing factory propagates the error, and the next request retries normally.
- Cache keys include kind, normalized term (lower-case, collapsed whitespace), and offset.
- Search terms are normalized before use as cache keys, so `"Funny Cats"` and `"funny  cats"` map to the same entry.

## Limitations and future work

| Item | Notes |
|---|---|
| Single-instance cache | L1 is per-process. Multi-instance deployments cache independently. Future: register `IDistributedCache` (Redis) to get HybridCache L2 and cross-instance coalescing. |
| Differentiated empty TTL | Empty results share the single TTL. HybridCache fixes options before the result is known, so a result-dependent TTL cannot be expressed without a separate caching layer. |
| Cross-page drift | Independently cached pages can overlap or miss an item as GIPHY rankings shift. Acceptable for ephemeral trending/search data. |
| Country / region | Not implemented. A TODO is marked in `IGifProvider`. Would become part of the request and cache key when introduced. |
| Retries / circuit breaker | Not included (retries multiply paid GIPHY calls). Named as a production enhancement. |
| Eager refresh | A background pre-warm was considered and rejected: a GIPHY beta key allows 100 calls/hour and a poller would exhaust that with zero users. |
