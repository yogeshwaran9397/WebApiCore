namespace WebCoreAPI.Exceptions;

/// <summary>
/// Base exception class for application-specific exceptions
/// </summary>
public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message) { }
    protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string field, string error) : base($"Validation failed for field '{field}'")
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { error }
        };
    }

    public ValidationException(Dictionary<string, string[]> errors) : base("One or more validation errors occurred")
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception for resource not found errors
/// </summary>
public class NotFoundException : BaseException
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public NotFoundException(string resourceType, string resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string message) : base(message)
    {
        ResourceType = "Resource";
        ResourceId = "Unknown";
    }
}

/// <summary>
/// Exception for forbidden access errors
/// </summary>
public class ForbiddenException : BaseException
{
    public string? RequiredRole { get; }
    public string? RequiredPermission { get; }

    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string message, string requiredRole) : base(message)
    {
        RequiredRole = requiredRole;
    }

    public ForbiddenException(string message, string requiredRole, string requiredPermission) : base(message)
    {
        RequiredRole = requiredRole;
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Exception for resource conflict errors
/// </summary>
public class ConflictException : BaseException
{
    public string? ConflictingResource { get; }

    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, string conflictingResource) : base(message)
    {
        ConflictingResource = conflictingResource;
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : BaseException
{
    public string RuleName { get; }
    public Dictionary<string, object> RuleParameters { get; }

    public BusinessRuleException(string ruleName, string message) : base(message)
    {
        RuleName = ruleName;
        RuleParameters = new Dictionary<string, object>();
    }

    public BusinessRuleException(string ruleName, string message, Dictionary<string, object> ruleParameters) : base(message)
    {
        RuleName = ruleName;
        RuleParameters = ruleParameters;
    }
}

/// <summary>
/// Exception for external service errors
/// </summary>
public class ExternalServiceException : BaseException
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public ExternalServiceException(string serviceName, string message) : base(message)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, int statusCode) : base(message)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException) : base(message, innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception for rate limiting errors
/// </summary>
public class RateLimitException : BaseException
{
    public TimeSpan RetryAfter { get; }
    public int RequestsPerWindow { get; }
    public TimeSpan WindowSize { get; }

    public RateLimitException(TimeSpan retryAfter, int requestsPerWindow, TimeSpan windowSize)
        : base($"Rate limit exceeded. Try again after {retryAfter.TotalSeconds} seconds.")
    {
        RetryAfter = retryAfter;
        RequestsPerWindow = requestsPerWindow;
        WindowSize = windowSize;
    }
}