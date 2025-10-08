using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates different API versioning techniques
/// Shows how the same controller can handle multiple versioning methods
/// </summary>
[ApiController]
[Route("api/versioning-demo")]
public class VersioningDemoController : ControllerBase
{
    private readonly ILogger<VersioningDemoController> _logger;

    public VersioningDemoController(ILogger<VersioningDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// URL Path Versioning Demo
    /// Accessible via: 
    /// - /api/v1/versioning-demo/url-path
    /// - /api/v2/versioning-demo/url-path
    /// </summary>
    [HttpGet("url-path")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/versioning-demo/url-path")]
    public IActionResult UrlPathVersioning()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        _logger.LogInformation("VersioningDemo: URL Path versioning - Version {Version}", apiVersion);

        object response = apiVersion?.MajorVersion switch
        {
            1 => new
            {
                VersioningMethod = "URL Path",
                Version = "1.0",
                Message = "This is Version 1.0 accessed via URL path",
                Example = "/api/v1/versioning-demo/url-path",
                Features = new[] { "Basic functionality", "Simple response structure" },
                Data = new { Id = 1, Name = "Basic Item", Type = "V1" }
            },
            2 => new
            {
                VersioningMethod = "URL Path", 
                Version = "2.0",
                Message = "This is Version 2.0 accessed via URL path",
                Example = "/api/v2/versioning-demo/url-path",
                Features = new[] { "Enhanced functionality", "Rich response structure", "Additional metadata" },
                Data = new 
                { 
                    Id = 1, 
                    Name = "Enhanced Item", 
                    Type = "V2",
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new { LastUpdated = DateTime.UtcNow, Source = "V2 API" }
                },
                Pagination = new { Page = 1, Size = 10, Total = 1 }
            },
            _ => new
            {
                VersioningMethod = "URL Path",
                Version = "Unknown",
                Message = "Unknown version requested",
                RequestedVersion = apiVersion?.ToString()
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Query String Versioning Demo
    /// Accessible via:
    /// - /api/versioning-demo/query-string?version=1.0
    /// - /api/versioning-demo/query-string?version=2.0
    /// </summary>
    [HttpGet("query-string")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public IActionResult QueryStringVersioning()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        var versionFromQuery = Request.Query["version"].ToString();
        
        _logger.LogInformation("VersioningDemo: Query String versioning - Version {Version}, Query: {QueryVersion}", 
            apiVersion, versionFromQuery);

        object response = apiVersion?.MajorVersion switch
        {
            1 => new
            {
                VersioningMethod = "Query String",
                Version = "1.0",
                Message = "This is Version 1.0 accessed via query string",
                Example = "/api/versioning-demo/query-string?version=1.0",
                QueryParameter = versionFromQuery,
                Features = new[] { "Query-based versioning", "Easy to use", "URL parameter" },
                Data = new[] 
                {
                    new { Id = 1, Title = "Item 1" },
                    new { Id = 2, Title = "Item 2" }
                }
            },
            2 => new
            {
                VersioningMethod = "Query String",
                Version = "2.0", 
                Message = "This is Version 2.0 accessed via query string",
                Example = "/api/versioning-demo/query-string?version=2.0",
                QueryParameter = versionFromQuery,
                Features = new[] { "Advanced query versioning", "Backward compatibility", "Enhanced responses" },
                Data = new[]
                {
                    new { 
                        Id = 1, 
                        Title = "Enhanced Item 1", 
                        Description = "Detailed description for item 1",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        Tags = new[] { "v2", "enhanced" }
                    },
                    new { 
                        Id = 2, 
                        Title = "Enhanced Item 2", 
                        Description = "Detailed description for item 2",
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        Tags = new[] { "v2", "improved" }
                    }
                },
                Metadata = new
                {
                    TotalItems = 2,
                    ResponseGenerated = DateTime.UtcNow,
                    ApiVersionUsed = apiVersion?.ToString()
                }
            },
            _ => new
            {
                VersioningMethod = "Query String",
                Version = "Unknown",
                Message = "Unknown version requested via query string",
                RequestedVersion = apiVersion?.ToString(),
                QueryParameter = versionFromQuery
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Header Versioning Demo
    /// Accessible via:
    /// - Header: X-Version: 1.0
    /// - Header: X-Version: 2.0
    /// </summary>
    [HttpGet("header")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public IActionResult HeaderVersioning()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        var versionFromHeader = Request.Headers["X-Version"].ToString();
        
        _logger.LogInformation("VersioningDemo: Header versioning - Version {Version}, Header: {HeaderVersion}", 
            apiVersion, versionFromHeader);

        object response = apiVersion?.MajorVersion switch
        {
            1 => new
            {
                VersioningMethod = "Header",
                Version = "1.0",
                Message = "This is Version 1.0 accessed via X-Version header",
                Example = "curl -H 'X-Version: 1.0' /api/versioning-demo/header",
                HeaderValue = versionFromHeader,
                Features = new[] { "Clean URLs", "Header-based versioning", "RESTful approach" },
                RequestHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Data = new
                {
                    Status = "Success",
                    Items = new[] { "Item A", "Item B", "Item C" }
                }
            },
            2 => new
            {
                VersioningMethod = "Header",
                Version = "2.0",
                Message = "This is Version 2.0 accessed via X-Version header", 
                Example = "curl -H 'X-Version: 2.0' /api/versioning-demo/header",
                HeaderValue = versionFromHeader,
                Features = new[] { "Advanced header versioning", "Rich metadata", "Enhanced security" },
                RequestHeaders = Request.Headers
                    .Where(h => h.Key.StartsWith("X-") || h.Key == "Accept" || h.Key == "Content-Type")
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                Data = new
                {
                    Status = "Success",
                    Items = new[]
                    {
                        new { Id = "A", Name = "Enhanced Item A", Priority = "High", LastModified = DateTime.UtcNow.AddHours(-2) },
                        new { Id = "B", Name = "Enhanced Item B", Priority = "Medium", LastModified = DateTime.UtcNow.AddHours(-1) },
                        new { Id = "C", Name = "Enhanced Item C", Priority = "Low", LastModified = DateTime.UtcNow.AddMinutes(-30) }
                    }
                },
                Metadata = new
                {
                    ResponseTime = DateTime.UtcNow,
                    ProcessingTimeMs = new Random().Next(10, 100),
                    ServerVersion = "2.0.1"
                }
            },
            _ => new
            {
                VersioningMethod = "Header", 
                Version = "Unknown",
                Message = "Unknown version requested via header",
                RequestedVersion = apiVersion?.ToString(),
                HeaderValue = versionFromHeader,
                AvailableVersions = new[] { "1.0", "2.0" }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Media Type Versioning Demo
    /// Accessible via:
    /// - Accept: application/json;version=1.0
    /// - Accept: application/json;version=2.0
    /// </summary>
    [HttpGet("media-type")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public IActionResult MediaTypeVersioning()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        var acceptHeader = Request.Headers["Accept"].ToString();
        
        _logger.LogInformation("VersioningDemo: Media Type versioning - Version {Version}, Accept: {AcceptHeader}", 
            apiVersion, acceptHeader);

        object response = apiVersion?.MajorVersion switch
        {
            1 => new
            {
                VersioningMethod = "Media Type",
                Version = "1.0",
                Message = "This is Version 1.0 accessed via Accept header media type",
                Example = "curl -H 'Accept: application/json;version=1.0' /api/versioning-demo/media-type",
                AcceptHeader = acceptHeader,
                Features = new[] { "Content negotiation", "Media type versioning", "HTTP standard compliant" },
                ContentType = "application/json;version=1.0",
                Data = new
                {
                    Format = "JSON",
                    Schema = "Basic",
                    Fields = new[] { "id", "name", "value" },
                    SampleData = new { Id = 1, Name = "Sample", Value = "Basic Value" }
                }
            },
            2 => new
            {
                VersioningMethod = "Media Type",
                Version = "2.0",
                Message = "This is Version 2.0 accessed via Accept header media type",
                Example = "curl -H 'Accept: application/json;version=2.0' /api/versioning-demo/media-type",
                AcceptHeader = acceptHeader,
                Features = new[] { "Advanced content negotiation", "Enhanced media types", "Backward compatibility" },
                ContentType = "application/json;version=2.0",
                Data = new
                {
                    Format = "JSON",
                    Schema = "Enhanced",
                    Fields = new[] { "id", "name", "value", "metadata", "relationships" },
                    SampleData = new 
                    { 
                        Id = 1, 
                        Name = "Enhanced Sample", 
                        Value = "Advanced Value",
                        Metadata = new
                        {
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow.AddDays(-1),
                            Version = "2.0",
                            Tags = new[] { "enhanced", "v2" }
                        },
                        Relationships = new
                        {
                            ParentId = (int?)null,
                            ChildrenCount = 0,
                            RelatedItems = new string[] { }
                        }
                    }
                },
                Schema = new
                {
                    Version = "2.0",
                    LastUpdated = DateTime.UtcNow.AddDays(-7),
                    BreakingChanges = new[] { "Added metadata object", "Added relationships object" },
                    Compatibility = "Backward compatible with 1.0 for basic fields"
                }
            },
            _ => new
            {
                VersioningMethod = "Media Type",
                Version = "Unknown",
                Message = "Unknown version requested via media type",
                RequestedVersion = apiVersion?.ToString(),
                AcceptHeader = acceptHeader,
                SupportedMediaTypes = new[] 
                { 
                    "application/json;version=1.0", 
                    "application/json;version=2.0" 
                }
            }
        };

        // Set the response content type to include version
        var contentType = apiVersion?.MajorVersion switch
        {
            1 => "application/json;version=1.0",
            2 => "application/json;version=2.0", 
            _ => "application/json"
        };
        
        Response.ContentType = contentType;
        return Ok(response);
    }

    /// <summary>
    /// Combined Versioning Demo - Shows how multiple versioning methods can work together
    /// Supports all versioning methods simultaneously
    /// </summary>
    [HttpGet("combined")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/versioning-demo/combined")] // URL path support
    public IActionResult CombinedVersioning()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        
        // Extract version information from different sources
        var versionSources = new
        {
            RequestedVersion = apiVersion?.ToString(),
            UrlPath = RouteData.Values["version"]?.ToString(),
            QueryString = Request.Query["version"].ToString(),
            Header = Request.Headers["X-Version"].ToString(),
            MediaType = ExtractVersionFromAcceptHeader(Request.Headers["Accept"].ToString())
        };

        _logger.LogInformation("VersioningDemo: Combined versioning - Version {Version}, Sources: {@Sources}", 
            apiVersion, versionSources);

        var response = new
        {
            VersioningMethod = "Combined (All Methods)",
            Version = apiVersion?.ToString(),
            Message = "This endpoint supports all versioning methods simultaneously",
            DetectedVersionSources = versionSources,
            Priority = new[]
            {
                "1. URL Path (highest priority)",
                "2. Query String", 
                "3. Header",
                "4. Media Type (lowest priority)"
            },
            Examples = new
            {
                UrlPath = "/api/v2/versioning-demo/combined",
                QueryString = "/api/versioning-demo/combined?version=2.0",
                Header = "curl -H 'X-Version: 2.0' /api/versioning-demo/combined",
                MediaType = "curl -H 'Accept: application/json;version=2.0' /api/versioning-demo/combined"
            },
            Data = (object)(apiVersion?.MajorVersion >= 2 
                ? new { Enhanced = true, Features = new[] { "All versioning methods", "Priority resolution", "Flexible access" } }
                : new { Basic = true, Features = new[] { "Multiple access methods", "Version detection" } }),
            Metadata = new
            {
                RequestProcessedAt = DateTime.UtcNow,
                VersionResolutionMethod = DetermineVersionSource(versionSources),
                AllRequestHeaders = Request.Headers
                    .Where(h => h.Key.Contains("Version") || h.Key == "Accept")
                    .ToDictionary(h => h.Key, h => h.Value.ToString())
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Version Information and Capabilities
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetVersioningInfo()
    {
        _logger.LogInformation("VersioningDemo: Getting versioning information");

        return Ok(new
        {
            Title = "API Versioning Demonstration",
            Description = "This controller demonstrates various API versioning techniques supported by ASP.NET Core",
            SupportedVersions = new[] { "1.0", "2.0" },
            DefaultVersion = "1.0",
            VersioningMethods = new
            {
                UrlPath = new
                {
                    Description = "Version specified in the URL path",
                    Example = "/api/v2/versioning-demo/url-path",
                    Pros = new[] { "Very explicit", "Easy to cache", "RESTful" },
                    Cons = new[] { "URL proliferation", "Harder to maintain" }
                },
                QueryString = new
                {
                    Description = "Version specified as query parameter",
                    Example = "/api/versioning-demo/query-string?version=2.0",
                    Pros = new[] { "Easy to implement", "Optional parameter", "Flexible" },
                    Cons = new[] { "Can be overlooked", "Not RESTful" }
                },
                Header = new
                {
                    Description = "Version specified in custom header",
                    Example = "X-Version: 2.0",
                    Pros = new[] { "Clean URLs", "Out-of-band versioning", "Flexible" },
                    Cons = new[] { "Hidden from URL", "Requires documentation" }
                },
                MediaType = new
                {
                    Description = "Version specified in Accept header",
                    Example = "Accept: application/json;version=2.0",
                    Pros = new[] { "HTTP standard", "Content negotiation", "Precise" },
                    Cons = new[] { "Complex", "Limited client support" }
                }
            },
            TestEndpoints = new[]
            {
                "/api/versioning-demo/url-path",
                "/api/versioning-demo/query-string",
                "/api/versioning-demo/header",
                "/api/versioning-demo/media-type",
                "/api/versioning-demo/combined",
                "/api/versioning-demo/info"
            },
            ConfigurationInfo = new
            {
                DefaultVersion = "1.0",
                AssumeDefaultWhenUnspecified = true,
                VersionReaders = new[] { "UrlSegment", "QueryString", "Header", "MediaType" },
                GroupNameFormat = "'v'VVV"
            }
        });
    }

    private static string ExtractVersionFromAcceptHeader(string acceptHeader)
    {
        if (string.IsNullOrEmpty(acceptHeader)) return string.Empty;
        
        var versionIndex = acceptHeader.IndexOf("version=");
        if (versionIndex == -1) return string.Empty;
        
        var versionStart = versionIndex + "version=".Length;
        var versionEnd = acceptHeader.IndexOf(';', versionStart);
        if (versionEnd == -1) versionEnd = acceptHeader.IndexOf(',', versionStart);
        if (versionEnd == -1) versionEnd = acceptHeader.Length;
        
        return acceptHeader.Substring(versionStart, versionEnd - versionStart);
    }

    private static string DetermineVersionSource(object versionSources)
    {
        var sources = versionSources.GetType().GetProperties()
            .Where(p => p.Name != "RequestedVersion")
            .Select(p => new { Name = p.Name, Value = p.GetValue(versionSources)?.ToString() })
            .Where(s => !string.IsNullOrEmpty(s.Value))
            .ToList();

        if (!sources.Any()) return "Default (no version specified)";
        
        // Priority order: URL Path, Query String, Header, Media Type
        var priorityOrder = new[] { "UrlPath", "QueryString", "Header", "MediaType" };
        
        foreach (var priority in priorityOrder)
        {
            var source = sources.FirstOrDefault(s => s.Name == priority);
            if (source != null) return source.Name;
        }
        
        return sources.First().Name;
    }
}