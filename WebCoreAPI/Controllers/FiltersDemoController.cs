using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Filters;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates the filter pipeline. Watch the console to see the order in which
/// the Resource, Action, and Result filters fire around the action method.
///
/// ServiceFilter vs TypeFilter:
///   [ServiceFilter] — the filter is resolved from DI (must be registered).
///   [TypeFilter]    — the filter is created by DI on demand and CAN take arguments.
/// </summary>
[ApiController]
[Route("api/filters")]
[ServiceFilter(typeof(TimingResourceFilter))] // controller-level resource filter
public class FiltersDemoController : ControllerBase
{
    /// <summary>
    /// All four "happy path" filters fire here. Expected console order:
    /// ResourceFilter(before) -> ActionFilter(before) -> [action] ->
    /// ActionFilter(after) -> ResultFilter -> ResourceFilter(after).
    /// </summary>
    [HttpGet("pipeline")]
    [ServiceFilter(typeof(LoggingActionFilter))]
    [TypeFilter(typeof(CustomResultFilter))]
    public IActionResult Pipeline()
        => Ok(new { message = "Check the console for filter order, and the X-Demo-Result-Filter response header." });

    /// <summary>
    /// This action throws on purpose; the DemoExceptionFilter catches it and
    /// returns a clean 500 instead of an unhandled crash.
    /// </summary>
    [HttpGet("exception")]
    [TypeFilter(typeof(DemoExceptionFilter))]
    public IActionResult ThrowsException()
        => throw new InvalidOperationException("Boom! This is caught by DemoExceptionFilter.");
}
