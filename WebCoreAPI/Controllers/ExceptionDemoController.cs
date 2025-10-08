using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Exceptions;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Exception Handling Demo Controller
/// Demonstrates various types of exceptions and how they are handled by the global exception middleware
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/exception-demo")]
[Tags("Exception Handling")]
public class ExceptionDemoController : ControllerBase
{
    private readonly ILogger<ExceptionDemoController> _logger;

    public ExceptionDemoController(ILogger<ExceptionDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get exception handling information
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public IActionResult GetExceptionInfo()
    {
        return Ok(new
        {
            Message = "Exception Handling Demo",
            Description = "This controller demonstrates various exception types and how they are handled globally",
            AvailableExceptions = new[]
            {
                new { Endpoint = "/validation-error", Type = "ValidationException", Status = 400 },
                new { Endpoint = "/not-found", Type = "NotFoundException", Status = 404 },
                new { Endpoint = "/unauthorized", Type = "UnauthorizedAccessException", Status = 401 },
                new { Endpoint = "/forbidden", Type = "ForbiddenException", Status = 403 },
                new { Endpoint = "/conflict", Type = "ConflictException", Status = 409 },
                new { Endpoint = "/timeout", Type = "TimeoutException", Status = 408 },
                new { Endpoint = "/business-rule", Type = "BusinessRuleException", Status = 400 },
                new { Endpoint = "/external-service", Type = "ExternalServiceException", Status = 502 },
                new { Endpoint = "/rate-limit", Type = "RateLimitException", Status = 429 },
                new { Endpoint = "/internal-error", Type = "Exception", Status = 500 }
            },
            Usage = "Call any of the endpoints above to see how different exceptions are handled",
            GlobalMiddleware = "All exceptions are caught and processed by GlobalExceptionHandlingMiddleware"
        });
    }

    /// <summary>
    /// Trigger a validation exception
    /// </summary>
    [HttpPost("validation-error")]
    [AllowAnonymous]
    public IActionResult TriggerValidationError([FromBody] TestModel? model)
    {
        var errors = new Dictionary<string, string[]>();
        
        if (string.IsNullOrEmpty(model?.Name))
        {
            errors.Add("name", new[] { "Name is required", "Name must be at least 2 characters long" });
        }
        
        if (model?.Age < 0 || model?.Age > 120)
        {
            errors.Add("age", new[] { "Age must be between 0 and 120" });
        }
        
        if (string.IsNullOrEmpty(model?.Email) || !model.Email.Contains("@"))
        {
            errors.Add("email", new[] { "Valid email is required" });
        }

        if (errors.Any())
        {
            throw new ValidationException("Validation failed for the provided data", errors);
        }

        return Ok(new { Message = "Validation passed!", Data = model });
    }

    /// <summary>
    /// Trigger a not found exception
    /// </summary>
    [HttpGet("not-found/{id}")]
    [AllowAnonymous]
    public IActionResult TriggerNotFoundException(int id)
    {
        _logger.LogInformation("Attempting to find resource with ID: {Id}", id);
        
        // Simulate resource lookup
        if (id <= 0)
        {
            throw new NotFoundException("Book", id.ToString());
        }

        // Always throw not found for demonstration
        throw new NotFoundException("Book", id.ToString());
    }

    /// <summary>
    /// Trigger an unauthorized exception
    /// </summary>
    [HttpGet("unauthorized")]
    [AllowAnonymous]
    public IActionResult TriggerUnauthorizedException()
    {
        _logger.LogWarning("Unauthorized access attempt detected");
        throw new UnauthorizedAccessException("Authentication required to access this resource");
    }

    /// <summary>
    /// Trigger a forbidden exception
    /// </summary>
    [HttpGet("forbidden")]
    [Authorize]
    public IActionResult TriggerForbiddenException()
    {
        var username = User.Identity?.Name;
        _logger.LogWarning("Forbidden access attempt by user: {Username}", username);
        
        throw new ForbiddenException(
            "Insufficient privileges to access this resource", 
            "Admin", 
            "system.admin");
    }

    /// <summary>
    /// Trigger a conflict exception
    /// </summary>
    [HttpPost("conflict")]
    [AllowAnonymous]
    public IActionResult TriggerConflictException([FromBody] CreateUserRequest request)
    {
        _logger.LogWarning("Conflict detected for username: {Username}", request?.Username);
        
        throw new ConflictException(
            $"User with username '{request?.Username}' already exists", 
            request?.Username ?? "unknown");
    }

    /// <summary>
    /// Trigger a timeout exception
    /// </summary>
    [HttpGet("timeout")]
    [AllowAnonymous]
    public IActionResult TriggerTimeoutException()
    {
        _logger.LogWarning("Simulating timeout scenario");
        throw new TimeoutException("The external service request timed out after 30 seconds");
    }

    /// <summary>
    /// Trigger a business rule exception
    /// </summary>
    [HttpPost("business-rule")]
    [AllowAnonymous]
    public IActionResult TriggerBusinessRuleException([FromBody] OrderRequest request)
    {
        _logger.LogWarning("Business rule violation for order: {@OrderRequest}", request);
        
        var ruleParameters = new Dictionary<string, object>
        {
            ["requested_quantity"] = request?.Quantity ?? 0,
            ["available_stock"] = 5,
            ["max_order_quantity"] = 10,
            ["customer_credit_limit"] = 1000
        };

        throw new BusinessRuleException(
            "InsufficientStock", 
            "Cannot process order: insufficient stock available", 
            ruleParameters);
    }

    /// <summary>
    /// Trigger an external service exception
    /// </summary>
    [HttpGet("external-service")]
    [AllowAnonymous]
    public IActionResult TriggerExternalServiceException()
    {
        _logger.LogError("External service communication failed");
        
        throw new ExternalServiceException(
            "PaymentGateway", 
            "Payment processing service is currently unavailable", 
            503);
    }

    /// <summary>
    /// Trigger a rate limit exception
    /// </summary>
    [HttpGet("rate-limit")]
    [AllowAnonymous]
    public IActionResult TriggerRateLimitException()
    {
        _logger.LogWarning("Rate limit exceeded for IP: {RemoteIP}", 
            HttpContext.Connection.RemoteIpAddress);
        
        throw new RateLimitException(
            TimeSpan.FromMinutes(15), // Retry after 15 minutes
            100, // 100 requests
            TimeSpan.FromHours(1) // per hour
        );
    }

    /// <summary>
    /// Trigger a generic internal server error
    /// </summary>
    [HttpGet("internal-error")]
    [AllowAnonymous]
    public IActionResult TriggerInternalError()
    {
        _logger.LogError("Simulating internal server error");
        
        // Simulate an unexpected error
        throw new InvalidOperationException("Something went wrong in the internal processing logic");
    }

    /// <summary>
    /// Trigger a null reference exception (unhandled exception type)
    /// </summary>
    [HttpGet("null-reference")]
    [AllowAnonymous]
    public IActionResult TriggerNullReferenceException()
    {
        _logger.LogError("Simulating null reference exception");
        
        string? nullString = null;
        // This will throw a NullReferenceException
        return Ok(nullString.Length);
    }

    /// <summary>
    /// Trigger an argument exception
    /// </summary>
    [HttpGet("argument-error/{value}")]
    [AllowAnonymous]
    public IActionResult TriggerArgumentException(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("Value cannot exceed 100 characters", nameof(value));
        }

        return Ok(new { Message = "Value is valid", Value = value });
    }

    /// <summary>
    /// Test exception with correlation ID
    /// </summary>
    [HttpGet("with-correlation/{correlationId}")]
    [AllowAnonymous]
    public IActionResult TriggerExceptionWithCorrelation(string correlationId)
    {
        // Add correlation ID to request headers
        HttpContext.Request.Headers["X-Correlation-ID"] = correlationId;
        
        _logger.LogError("Error with correlation ID: {CorrelationId}", correlationId);
        throw new InvalidOperationException($"Test exception with correlation ID: {correlationId}");
    }

    /// <summary>
    /// Test authenticated user exception
    /// </summary>
    [HttpGet("authenticated-error")]
    [Authorize]
    public IActionResult TriggerAuthenticatedUserException()
    {
        var username = User.Identity?.Name;
        var userId = User.FindFirst("user_id")?.Value;
        
        _logger.LogError("Authenticated user error - Username: {Username}, UserId: {UserId}", 
            username, userId);
        
        throw new InvalidOperationException("An error occurred while processing the authenticated user request");
    }

    /// <summary>
    /// Test multiple exceptions (demonstrate exception aggregation)
    /// </summary>
    [HttpPost("multiple-errors")]
    [AllowAnonymous]
    public IActionResult TriggerMultipleErrors([FromBody] ComplexModel model)
    {
        var exceptions = new List<Exception>();

        try
        {
            ValidateModel(model);
        }
        catch (ValidationException ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            ProcessBusinessRules(model);
        }
        catch (BusinessRuleException ex)
        {
            exceptions.Add(ex);
        }

        if (exceptions.Count > 0)
        {
            // Throw the first exception (middleware will handle it)
            throw exceptions.First();
        }

        return Ok(new { Message = "All validations passed", Data = model });
    }

    #region Helper Methods and Models

    private void ValidateModel(ComplexModel? model)
    {
        if (model == null)
        {
            throw new ValidationException("model", "Model is required");
        }

        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrEmpty(model.Name))
        {
            errors.Add("name", new[] { "Name is required" });
        }

        if (model.Price < 0)
        {
            errors.Add("price", new[] { "Price cannot be negative" });
        }

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }
    }

    private void ProcessBusinessRules(ComplexModel model)
    {
        if (model.Price > 10000)
        {
            throw new BusinessRuleException(
                "PriceLimit",
                "Price exceeds maximum allowed limit",
                new Dictionary<string, object> { ["max_price"] = 10000, ["provided_price"] = model.Price });
        }
    }

    #endregion
}

#region Request Models

public class TestModel
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public string? Email { get; set; }
}

public class CreateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class OrderRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ComplexModel
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public DateTime? CreatedDate { get; set; }
}

#endregion