using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebCoreAPI.Filters;

// ---------------------------------------------------------------------------
// The filter pipeline runs in this order around an action:
//
//   Authorization -> Resource -> [Model Binding] -> Action -> (ACTION RUNS)
//                 -> Action(after) -> Result -> (RESULT RUNS) -> Result(after)
//   Exception filters catch unhandled exceptions thrown by the action/result.
// ---------------------------------------------------------------------------

/// <summary>
/// ACTION FILTER — runs immediately before and after the action method.
/// Great for logging, timing, or inspecting/modifying arguments and results.
/// </summary>
public class LoggingActionFilter : IActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;
    public LoggingActionFilter(ILogger<LoggingActionFilter> logger) => _logger = logger;

    public void OnActionExecuting(ActionExecutingContext context)
        => _logger.LogInformation("[ActionFilter] BEFORE {Action}", context.ActionDescriptor.DisplayName);

    public void OnActionExecuted(ActionExecutedContext context)
        => _logger.LogInformation("[ActionFilter] AFTER {Action}", context.ActionDescriptor.DisplayName);
}

/// <summary>
/// RESOURCE FILTER — runs very early (before model binding) and last on the way out.
/// Classic use: short-circuit with a cached response to skip the whole pipeline.
/// </summary>
public class TimingResourceFilter : IResourceFilter
{
    private readonly ILogger<TimingResourceFilter> _logger;
    public TimingResourceFilter(ILogger<TimingResourceFilter> logger) => _logger = logger;

    public void OnResourceExecuting(ResourceExecutingContext context)
        => _logger.LogInformation("[ResourceFilter] Pipeline starting (before model binding).");

    public void OnResourceExecuted(ResourceExecutedContext context)
        => _logger.LogInformation("[ResourceFilter] Pipeline finished.");
}

/// <summary>
/// RESULT FILTER — runs before and after the IActionResult is executed.
/// Use it to wrap/modify the final response (e.g. add a custom header).
/// </summary>
public class CustomResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
        => context.HttpContext.Response.Headers["X-Demo-Result-Filter"] = "applied";

    public void OnResultExecuted(ResultExecutedContext context) { }
}

/// <summary>
/// EXCEPTION FILTER — catches unhandled exceptions from the action and turns
/// them into a clean response. (Global middleware is usually preferred, but this
/// shows the filter-level mechanism.)
/// </summary>
public class DemoExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DemoExceptionFilter> _logger;
    public DemoExceptionFilter(ILogger<DemoExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "[ExceptionFilter] Caught: {Message}", context.Exception.Message);
        context.Result = new ObjectResult(new
        {
            error = "Handled by DemoExceptionFilter",
            detail = context.Exception.Message
        })
        { StatusCode = StatusCodes.Status500InternalServerError };
        context.ExceptionHandled = true;
    }
}
