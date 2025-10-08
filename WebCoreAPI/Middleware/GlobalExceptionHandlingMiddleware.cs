using System.Net;
using System.Text.Json;
using WebCoreAPI.Exceptions;

namespace WebCoreAPI.Middleware;

/// <summary>
/// Global Exception Handling Middleware
/// Catches all unhandled exceptions and provides consistent error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}",
                context.TraceIdentifier, context.Request.Path, context.Request.Method);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            RequestId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path.Value ?? string.Empty,
            Method = context.Request.Method
        };

        // Determine status code and message based on exception type
        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Validation Error";
                response.Detail = validationEx.Message;
                response.Errors = validationEx.Errors;
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized";
                response.Detail = "Access denied. Please authenticate and try again.";
                break;

            case ForbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Title = "Forbidden";
                response.Detail = "You don't have permission to access this resource.";
                break;

            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Title = "Resource Not Found";
                response.Detail = exception.Message;
                break;

            case ConflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Title = "Conflict";
                response.Detail = exception.Message;
                break;

            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Title = "Request Timeout";
                response.Detail = "The request timed out. Please try again later.";
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Detail = argEx.Message;
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Invalid Operation";
                response.Detail = exception.Message;
                break;

            case NotSupportedException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Not Supported";
                response.Detail = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                response.Detail = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An error occurred while processing your request.";
                break;
        }

        // Add stack trace in development environment
        if (_environment.IsDevelopment())
        {
            response.StackTrace = exception.StackTrace;
            response.InnerException = exception.InnerException?.Message;
        }

        // Add additional context for specific exceptions
        AddAdditionalContext(response, exception, context);

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private void AddAdditionalContext(ErrorResponse response, Exception exception, HttpContext context)
    {
        // Add correlation ID if available
        if (context.Request.Headers.ContainsKey("X-Correlation-ID"))
        {
            response.CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        }

        // Add user context if authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            response.UserId = context.User.FindFirst("user_id")?.Value;
            response.Username = context.User.Identity.Name;
        }

        // Add request information for debugging
        response.RequestInfo = new RequestInfo
        {
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            QueryParameters = context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
            UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            ContentType = context.Request.ContentType
        };

        // Add specific error codes for different exception types
        response.ErrorCode = exception.GetType().Name switch
        {
            nameof(ValidationException) => "VALIDATION_FAILED",
            nameof(UnauthorizedAccessException) => "UNAUTHORIZED_ACCESS",
            nameof(ForbiddenException) => "ACCESS_FORBIDDEN",
            nameof(NotFoundException) => "RESOURCE_NOT_FOUND",
            nameof(ConflictException) => "RESOURCE_CONFLICT",
            nameof(TimeoutException) => "REQUEST_TIMEOUT",
            nameof(ArgumentException) => "INVALID_ARGUMENT",
            nameof(InvalidOperationException) => "INVALID_OPERATION",
            nameof(NotSupportedException) => "OPERATION_NOT_SUPPORTED",
            _ => "INTERNAL_ERROR"
        };

        // Add helpful error links or documentation references
        response.HelpLink = GetHelpLink(exception);
    }

    private string? GetHelpLink(Exception exception)
    {
        return exception.GetType().Name switch
        {
            nameof(ValidationException) => "https://docs.company.com/api/validation-errors",
            nameof(UnauthorizedAccessException) => "https://docs.company.com/api/authentication",
            nameof(ForbiddenException) => "https://docs.company.com/api/authorization",
            nameof(NotFoundException) => "https://docs.company.com/api/resources",
            _ => "https://docs.company.com/api/error-handling"
        };
    }
}

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public string? HelpLink { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public RequestInfo? RequestInfo { get; set; }
}

/// <summary>
/// Request information for debugging
/// </summary>
public class RequestInfo
{
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public string? UserAgent { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? ContentType { get; set; }
}