# Global Exception Handling Guide

## Overview

This .NET Core Web API implements a comprehensive global exception handling system that provides:

1. **Centralized Error Handling** - All exceptions are caught and processed consistently
2. **Standardized Error Responses** - Uniform error response format across the API
3. **Detailed Logging** - Comprehensive logging with context information
4. **Environment-Aware Responses** - Different levels of detail for development vs. production
5. **Custom Exception Types** - Domain-specific exceptions with rich metadata

## Architecture

### Global Exception Middleware

The `GlobalExceptionHandlingMiddleware` is the core component that:
- Catches all unhandled exceptions in the request pipeline
- Maps exception types to appropriate HTTP status codes
- Creates standardized error responses
- Logs exceptions with context information
- Handles security considerations (hiding sensitive data in production)

### Middleware Registration

The middleware is registered early in the pipeline in `Program.cs`:

```csharp
app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>(); // First middleware
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
```

## Exception Types & HTTP Status Codes

| Exception Type | HTTP Status | Description | Example Use Case |
|---------------|-------------|-------------|------------------|
| `ValidationException` | 400 Bad Request | Input validation errors | Invalid form data |
| `ArgumentException` | 400 Bad Request | Invalid method arguments | Missing required parameters |
| `NotFoundException` | 404 Not Found | Resource not found | Book ID doesn't exist |
| `UnauthorizedAccessException` | 401 Unauthorized | Authentication required | Missing JWT token |
| `ForbiddenException` | 403 Forbidden | Insufficient permissions | User lacks required role |
| `ConflictException` | 409 Conflict | Resource conflict | Username already exists |
| `TimeoutException` | 408 Request Timeout | Operation timeout | External service timeout |
| `BusinessRuleException` | 400 Bad Request | Business logic violation | Insufficient stock |
| `ExternalServiceException` | 502 Bad Gateway | External service error | Payment gateway down |
| `RateLimitException` | 429 Too Many Requests | Rate limiting | API quota exceeded |
| `Exception` (Generic) | 500 Internal Server Error | Unexpected errors | Null reference, etc. |

## Custom Exception Classes

### Base Exception
```csharp
public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message) { }
    protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}
```

### Validation Exception
```csharp
public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred")
    {
        Errors = errors;
    }
}
```

### Not Found Exception
```csharp
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
}
```

### Business Rule Exception
```csharp
public class BusinessRuleException : BaseException
{
    public string RuleName { get; }
    public Dictionary<string, object> RuleParameters { get; }
    
    public BusinessRuleException(string ruleName, string message, Dictionary<string, object> ruleParameters) 
        : base(message)
    {
        RuleName = ruleName;
        RuleParameters = ruleParameters;
    }
}
```

## Error Response Format

### Standard Error Response
```json
{
  "statusCode": 404,
  "title": "Resource Not Found",
  "detail": "Book with ID '999' was not found",
  "requestId": "0HMVP8L7QH9MK:00000001",
  "timestamp": "2025-10-06T15:30:45.123Z",
  "path": "/api/v1/exception-demo/not-found/999",
  "method": "GET",
  "errorCode": "RESOURCE_NOT_FOUND",
  "helpLink": "https://docs.company.com/api/resources"
}
```

### Validation Error Response
```json
{
  "statusCode": 400,
  "title": "Validation Error",
  "detail": "Validation failed for the provided data",
  "requestId": "0HMVP8L7QH9MK:00000002",
  "timestamp": "2025-10-06T15:31:15.456Z",
  "path": "/api/v1/exception-demo/validation-error",
  "method": "POST",
  "errorCode": "VALIDATION_FAILED",
  "errors": {
    "name": ["Name is required", "Name must be at least 2 characters long"],
    "age": ["Age must be between 0 and 120"],
    "email": ["Valid email is required"]
  }
}
```

### Authenticated User Error Response
```json
{
  "statusCode": 500,
  "title": "Internal Server Error",
  "detail": "An error occurred while processing the authenticated user request",
  "requestId": "0HMVP8L7QH9MK:00000003",
  "timestamp": "2025-10-06T15:32:00.789Z",
  "path": "/api/v1/exception-demo/authenticated-error",
  "method": "GET",
  "errorCode": "INTERNAL_ERROR",
  "userId": "1",
  "username": "admin",
  "requestInfo": {
    "headers": {
      "Authorization": "Bearer eyJ...",
      "Content-Type": "application/json"
    },
    "userAgent": "Mozilla/5.0...",
    "remoteIpAddress": "127.0.0.1"
  }
}
```

## Development vs Production

### Development Environment
- **Full exception details** including stack traces
- **Inner exception messages** for debugging
- **Complete request information** for troubleshooting
- **Detailed logging** with all context

### Production Environment
- **Generic error messages** to avoid information disclosure
- **No stack traces** to prevent sensitive information leakage
- **Minimal request information** for security
- **Sanitized logging** excluding sensitive data

## Exception Demo Controller

The `ExceptionDemoController` provides comprehensive testing endpoints for all exception types:

### Testing Endpoints

| Endpoint | Method | Description | Expected Status |
|----------|--------|-------------|-----------------|
| `/exception-demo/info` | GET | Exception system information | 200 |
| `/exception-demo/validation-error` | POST | Trigger validation exception | 400 |
| `/exception-demo/not-found/{id}` | GET | Trigger not found exception | 404 |
| `/exception-demo/unauthorized` | GET | Trigger unauthorized exception | 401 |
| `/exception-demo/forbidden` | GET | Trigger forbidden exception | 403 |
| `/exception-demo/conflict` | POST | Trigger conflict exception | 409 |
| `/exception-demo/timeout` | GET | Trigger timeout exception | 408 |
| `/exception-demo/business-rule` | POST | Trigger business rule exception | 400 |
| `/exception-demo/external-service` | GET | Trigger external service exception | 502 |
| `/exception-demo/rate-limit` | GET | Trigger rate limit exception | 429 |
| `/exception-demo/internal-error` | GET | Trigger internal server error | 500 |
| `/exception-demo/null-reference` | GET | Trigger null reference exception | 500 |
| `/exception-demo/argument-error/{value}` | GET | Trigger argument exception | 400 |
| `/exception-demo/with-correlation/{id}` | GET | Test with correlation ID | 500 |
| `/exception-demo/authenticated-error` | GET | Test authenticated user error | 500 |

## Usage Examples

### Throwing Custom Exceptions

#### Validation Exception
```csharp
var errors = new Dictionary<string, string[]>
{
    ["email"] = new[] { "Email is required", "Email must be valid" },
    ["age"] = new[] { "Age must be between 18 and 100" }
};
throw new ValidationException(errors);
```

#### Not Found Exception
```csharp
var book = await _bookService.GetByIdAsync(bookId);
if (book == null)
{
    throw new NotFoundException("Book", bookId.ToString());
}
```

#### Business Rule Exception
```csharp
if (orderQuantity > availableStock)
{
    var ruleParams = new Dictionary<string, object>
    {
        ["requested_quantity"] = orderQuantity,
        ["available_stock"] = availableStock
    };
    throw new BusinessRuleException("InsufficientStock", 
        "Cannot fulfill order: insufficient stock", ruleParams);
}
```

#### Forbidden Exception
```csharp
if (!user.HasPermission("users.delete"))
{
    throw new ForbiddenException("Insufficient privileges to delete users", 
        "Admin", "users.delete");
}
```

### Testing with cURL

#### Test Validation Error
```bash
curl -X POST "http://localhost:5274/api/v1/exception-demo/validation-error" \
  -H "Content-Type: application/json" \
  -d '{"name": "", "age": -5, "email": "invalid"}'
```

#### Test Not Found
```bash
curl -X GET "http://localhost:5274/api/v1/exception-demo/not-found/999"
```

#### Test Authenticated Error
```bash
curl -X GET "http://localhost:5274/api/v1/exception-demo/authenticated-error" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Test with Correlation ID
```bash
curl -X GET "http://localhost:5274/api/v1/exception-demo/with-correlation/TEST-12345"
```

## Logging Integration

### Exception Logging
The middleware logs all exceptions with rich context:

```csharp
_logger.LogError(ex, "An unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}",
    context.TraceIdentifier, context.Request.Path, context.Request.Method);
```

### Log Structure
- **Exception details** with full stack trace
- **Request context** (path, method, headers)
- **User context** (if authenticated)
- **Correlation IDs** for tracing across services
- **Request/Response timing** information

## Best Practices

### 1. Exception Design
- **Use specific exception types** for different error scenarios
- **Include meaningful error messages** for developers and users
- **Provide actionable error information** with suggested solutions
- **Use correlation IDs** for tracing across distributed systems

### 2. Security Considerations
- **Never expose sensitive information** in error responses
- **Sanitize stack traces** in production environments
- **Log security-relevant exceptions** for monitoring
- **Use generic error messages** for authentication failures

### 3. Performance
- **Avoid exceptions for control flow** - use them only for exceptional cases
- **Cache error response templates** where possible
- **Minimize exception creation overhead** in hot paths
- **Use structured logging** for efficient log parsing

### 4. Monitoring & Alerting
- **Set up alerts** for high exception rates
- **Monitor specific exception types** that indicate system issues
- **Track error patterns** to identify recurring problems
- **Use health checks** to proactively detect issues

## Testing Strategy

### Unit Tests
- Test each exception type with appropriate scenarios
- Verify correct HTTP status codes and response formats
- Test exception mapping logic
- Validate error message construction

### Integration Tests  
- Test end-to-end exception handling through the middleware
- Verify logging output and format
- Test authentication context in error responses
- Validate production vs development response differences

### Load Tests
- Verify exception handling performance under load
- Test exception handling with concurrent requests
- Monitor memory usage during exception scenarios
- Validate logging performance impact

## Extending the System

### Adding New Exception Types
1. Create a new exception class inheriting from `BaseException`
2. Add exception mapping in `GlobalExceptionHandlingMiddleware`
3. Define appropriate HTTP status code and error response
4. Add test endpoints in `ExceptionDemoController`
5. Update documentation and help links

### Custom Error Response Fields
1. Extend the `ErrorResponse` class with new properties
2. Update the `HandleExceptionAsync` method to populate new fields
3. Consider backward compatibility with existing clients
4. Document new response format

### Integration with External Services
1. Create service-specific exception types
2. Add retry logic and circuit breakers
3. Implement correlation ID propagation
4. Add service health monitoring

This comprehensive exception handling system provides robust error management, detailed logging, and consistent user experience across the entire Web API.