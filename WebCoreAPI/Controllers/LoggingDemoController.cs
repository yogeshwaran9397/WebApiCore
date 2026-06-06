using Microsoft.AspNetCore.Mvc;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates the built-in ILogger and its 6 log levels.
/// Watch your console output after calling these endpoints.
///
///   Trace < Debug < Information < Warning < Error < Critical
/// (Lower levels are usually filtered out in production via appsettings.json.)
/// </summary>
[ApiController]
[Route("api/logging")]
public class LoggingDemoController : ControllerBase
{
    private readonly ILogger<LoggingDemoController> _logger;

    public LoggingDemoController(ILogger<LoggingDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Writes one message at every log level. Note the use of STRUCTURED logging
    /// ({Placeholder}) instead of string interpolation — this lets log sinks like
    /// Serilog/Seq/Elasticsearch index the values as searchable properties.
    /// </summary>
    [HttpGet("all-levels")]
    public IActionResult LogAllLevels()
    {
        var userId = 123;

        _logger.LogTrace("TRACE: entering LogAllLevels for user {UserId}", userId);
        _logger.LogDebug("DEBUG: diagnostic detail for user {UserId}", userId);
        _logger.LogInformation("INFORMATION: user {UserId} requested the log demo", userId);
        _logger.LogWarning("WARNING: this is just a demo warning for user {UserId}", userId);
        _logger.LogError("ERROR: simulated handled error for user {UserId}", userId);
        _logger.LogCritical("CRITICAL: simulated critical condition for user {UserId}", userId);

        return Ok(new
        {
            message = "Wrote one entry per level. Check the server console.",
            levels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" },
            tip = "Default config hides Trace/Debug. Adjust Logging:LogLevel in appsettings.json."
        });
    }

    /// <summary>
    /// Demonstrates logging an exception object (stack trace is preserved).
    /// </summary>
    [HttpGet("exception")]
    public IActionResult LogException()
    {
        try
        {
            throw new InvalidOperationException("Simulated failure for logging demo.");
        }
        catch (Exception ex)
        {
            // Pass the exception as the FIRST argument so the full stack trace is logged.
            _logger.LogError(ex, "Caught an exception while processing {Operation}", nameof(LogException));
            return Ok(new { message = "Exception was logged with its stack trace. Check the console." });
        }
    }

    /// <summary>
    /// Demonstrates logging scopes — group related logs with shared context.
    /// </summary>
    [HttpGet("scope")]
    public IActionResult LogWithScope()
    {
        using (_logger.BeginScope("OrderProcessing for {OrderId}", 9001))
        {
            _logger.LogInformation("Validating order");
            _logger.LogInformation("Charging payment");
            _logger.LogInformation("Order completed");
        }
        return Ok(new { message = "Logged three messages sharing the OrderId scope." });
    }
}
