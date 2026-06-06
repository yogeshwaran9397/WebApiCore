using Microsoft.AspNetCore.Mvc;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates the common HTTP status codes and the IActionResult helpers
/// ASP.NET Core gives you to return them.
///
///   2xx = Success   | 3xx = Redirect | 4xx = Client error | 5xx = Server error
/// </summary>
[ApiController]
[Route("api/status-codes")]
public class StatusCodesController : ControllerBase
{
    // ---------- 2xx SUCCESS ----------

    // 200 OK - standard success with a body
    [HttpGet("200")]
    public IActionResult Ok200() => Ok(new { message = "200 OK - here is your data." });

    // 201 Created - resource created; includes a Location header pointing to it
    [HttpPost("201")]
    public IActionResult Created201()
    {
        var created = new { id = 42, name = "New Resource" };
        return CreatedAtAction(nameof(Ok200), new { id = created.id }, created);
    }

    // 202 Accepted - request accepted, processing happens asynchronously later
    [HttpPost("202")]
    public IActionResult Accepted202() => Accepted(new { message = "202 Accepted - queued for background processing." });

    // 204 No Content - success but nothing to return (typical for DELETE/PUT)
    [HttpDelete("204")]
    public IActionResult NoContent204() => NoContent();

    // ---------- 3xx REDIRECT ----------

    // 301 Moved Permanently
    [HttpGet("301")]
    public IActionResult Moved301() => RedirectPermanent("/api/status-codes/200");

    // 302 Found (temporary redirect)
    [HttpGet("302")]
    public IActionResult Found302() => Redirect("/api/status-codes/200");

    // 304 Not Modified - tell client to use its cached copy
    [HttpGet("304")]
    public IActionResult NotModified304() => StatusCode(StatusCodes.Status304NotModified);

    // ---------- 4xx CLIENT ERRORS ----------

    // 400 Bad Request - invalid/missing data
    [HttpGet("400")]
    public IActionResult Bad400() => BadRequest(new { error = "400 Bad Request - your input was invalid." });

    // 401 Unauthorized - "Who are you?" (not authenticated)
    [HttpGet("401")]
    public IActionResult Unauthorized401() => Unauthorized(new { error = "401 Unauthorized - please log in." });

    // 403 Forbidden - "I know you, but you can't do this." (authenticated, not allowed)
    [HttpGet("403")]
    public IActionResult Forbidden403() => StatusCode(StatusCodes.Status403Forbidden,
        new { error = "403 Forbidden - you don't have permission." });

    // 404 Not Found
    [HttpGet("404")]
    public IActionResult NotFound404() => NotFound(new { error = "404 Not Found - resource does not exist." });

    // 405 Method Not Allowed (wrong verb for the route)
    [HttpGet("405")]
    public IActionResult MethodNotAllowed405() => StatusCode(StatusCodes.Status405MethodNotAllowed,
        new { error = "405 Method Not Allowed - wrong HTTP verb." });

    // ---------- 5xx SERVER ERRORS ----------

    // 500 Internal Server Error
    [HttpGet("500")]
    public IActionResult ServerError500() => StatusCode(StatusCodes.Status500InternalServerError,
        new { error = "500 Internal Server Error - something broke on our side." });

    // 501 Not Implemented
    [HttpGet("501")]
    public IActionResult NotImplemented501() => StatusCode(StatusCodes.Status501NotImplemented,
        new { error = "501 Not Implemented - this feature doesn't exist yet." });

    // 503 Service Unavailable
    [HttpGet("503")]
    public IActionResult Unavailable503() => StatusCode(StatusCodes.Status503ServiceUnavailable,
        new { error = "503 Service Unavailable - server is overloaded or in maintenance." });

    // 504 Gateway Timeout
    [HttpGet("504")]
    public IActionResult GatewayTimeout504() => StatusCode(StatusCodes.Status504GatewayTimeout,
        new { error = "504 Gateway Timeout - an upstream service didn't respond." });
}
