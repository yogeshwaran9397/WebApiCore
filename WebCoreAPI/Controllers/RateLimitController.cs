using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates the built-in SLIDING WINDOW rate limiter (configured in Program.cs).
///
/// Policy "sliding": 5 requests per 15-second window, split into 3 x 5s segments.
/// Call this faster than 5 times per 15s and you'll start getting
/// 429 Too Many Requests (with a Retry-After header). Wait ~5s and the oldest
/// segment expires, freeing capacity again — that's the "sliding" behaviour.
///
/// [EnableRateLimiting] scopes the limit to THIS controller only, so the rest of
/// the API (and the React client) is never throttled.
/// </summary>
[ApiController]
[Route("api/rate-limit")]
[EnableRateLimiting("sliding")]
public class RateLimitController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "✅ Request allowed.",
            timestamp = DateTime.UtcNow,
            policy = "Sliding window: 5 requests / 15s (3 segments of 5s).",
            tip = "Fire more than 5 of these within 15s to get 429 Too Many Requests."
        });
    }

    /// <summary>
    /// Same limiter, but explicitly disabled here so you can compare a throttled
    /// vs un-throttled endpoint side by side.
    /// </summary>
    [HttpGet("unlimited")]
    [DisableRateLimiting]
    public IActionResult Unlimited()
        => Ok(new { message = "♾️ This endpoint ignores the rate limit (DisableRateLimiting)." });
}
