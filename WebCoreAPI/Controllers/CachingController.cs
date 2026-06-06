using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates two caching strategies:
///   1. In-Memory Cache  -> store expensive results in server RAM (IMemoryCache).
///   2. Response Caching  -> cache the whole HTTP response via Cache-Control headers.
/// </summary>
[ApiController]
[Route("api/caching")]
public class CachingController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "expensive_data";

    public CachingController(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// In-memory caching. The first call is "slow" (simulated work); subsequent calls
    /// within 30 seconds return the cached value instantly.
    /// Watch the GeneratedAt timestamp: it only changes after the cache expires.
    /// </summary>
    [HttpGet("memory")]
    public IActionResult GetWithMemoryCache()
    {
        if (_cache.TryGetValue(CacheKey, out object? cached))
        {
            return Ok(new { source = "MEMORY CACHE (hit)", data = cached });
        }

        // Cache miss: do the expensive work and store the result.
        var data = new { value = "Computed result", generatedAt = DateTime.UtcNow };
        _cache.Set(CacheKey, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15)
        });

        return Ok(new { source = "COMPUTED (cache miss)", data });
    }

    [HttpDelete("memory")]
    public IActionResult ClearMemoryCache()
    {
        _cache.Remove(CacheKey);
        return NoContent();
    }

    /// <summary>
    /// Response caching. The [ResponseCache] attribute adds a
    /// "Cache-Control: public, max-age=30" header so browsers/proxies/CDNs can
    /// reuse the response without calling the server again.
    /// </summary>
    [HttpGet("response")]
    [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
    public IActionResult GetWithResponseCache()
        => Ok(new { message = "This response is cacheable for 30s.", generatedAt = DateTime.UtcNow });

    /// <summary>
    /// Explicitly opt OUT of caching (sensitive data should never be cached).
    /// </summary>
    [HttpGet("no-cache")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetWithoutCache()
        => Ok(new { message = "Never cached.", generatedAt = DateTime.UtcNow });
}
