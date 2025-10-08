using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace WebCoreAPI.Controllers;

/// <summary>
/// This controller demonstrates ADVANCED ROUTING FEATURES
/// Including constraints, parameters, route generation, and special routing scenarios
/// </summary>
[ApiController]
[Route("advanced-routing")]
public class AdvancedRoutingController : ControllerBase
{
    private readonly ILogger<AdvancedRoutingController> _logger;

    public AdvancedRoutingController(ILogger<AdvancedRoutingController> logger)
    {
        _logger = logger;
    }

    // ============================================================================
    // ROUTE CONSTRAINTS EXAMPLES
    // ============================================================================

    /// <summary>
    /// Integer constraint with minimum and maximum values
    /// Route: /advanced-routing/age/{age}
    /// Constraint: age must be between 1 and 120
    /// </summary>
    [HttpGet("age/{age:int:range(1,120)}")]
    public IActionResult GetByAge(int age)
    {
        _logger.LogInformation("AdvancedRouting: Age constraint - {Age}", age);

        return Ok(new
        {
            Message = $"Age {age} is valid",
            Route = $"/advanced-routing/age/{age}",
            Constraint = "int:range(1,120)",
            ValidRange = "1 to 120",
            Age = age
        });
    }

    /// <summary>
    /// String length constraint
    /// Route: /advanced-routing/username/{username}
    /// Constraint: username must be 3-20 characters
    /// </summary>
    [HttpGet("username/{username:length(3,20)}")]
    public IActionResult GetByUsername(string username)
    {
        _logger.LogInformation("AdvancedRouting: Username constraint - {Username}", username);

        return Ok(new
        {
            Message = $"Username '{username}' is valid",
            Route = $"/advanced-routing/username/{username}",
            Constraint = "length(3,20)",
            Username = username,
            Length = username.Length
        });
    }

    /// <summary>
    /// GUID constraint
    /// Route: /advanced-routing/guid/{id}
    /// Constraint: id must be a valid GUID
    /// </summary>
    [HttpGet("guid/{id:guid}")]
    public IActionResult GetByGuid(Guid id)
    {
        _logger.LogInformation("AdvancedRouting: GUID constraint - {Id}", id);

        return Ok(new
        {
            Message = "Valid GUID received",
            Route = $"/advanced-routing/guid/{id}",
            Constraint = "guid",
            Id = id,
            Example = "/advanced-routing/guid/12345678-1234-1234-1234-123456789abc"
        });
    }

    /// <summary>
    /// DateTime constraint
    /// Route: /advanced-routing/date/{date}
    /// Constraint: date must be a valid DateTime
    /// </summary>
    [HttpGet("date/{date:datetime}")]
    public IActionResult GetByDate(DateTime date)
    {
        _logger.LogInformation("AdvancedRouting: DateTime constraint - {Date}", date);

        return Ok(new
        {
            Message = "Valid date received",
            Route = $"/advanced-routing/date/{date:yyyy-MM-dd}",
            Constraint = "datetime",
            Date = date,
            FormattedDate = date.ToString("yyyy-MM-dd HH:mm:ss"),
            Examples = new[]
            {
                "/advanced-routing/date/2023-12-25",
                "/advanced-routing/date/2023-12-25T10:30:00"
            }
        });
    }

    /// <summary>
    /// Decimal constraint with minimum value
    /// Route: /advanced-routing/price/{price}
    /// Constraint: price must be a decimal >= 0.01
    /// </summary>
    [HttpGet("price/{price:decimal:min(0.01)}")]
    public IActionResult GetByPrice(decimal price)
    {
        _logger.LogInformation("AdvancedRouting: Price constraint - {Price}", price);

        return Ok(new
        {
            Message = $"Price ${price:F2} is valid",
            Route = $"/advanced-routing/price/{price}",
            Constraint = "decimal:min(0.01)",
            Price = price,
            FormattedPrice = price.ToString("C", CultureInfo.GetCultureInfo("en-US"))
        });
    }

    /// <summary>
    /// Regex constraint for custom patterns
    /// Route: /advanced-routing/isbn/{isbn}
    /// Constraint: ISBN format (xxx-x-xxxx-xxxx-x)
    /// </summary>
    [HttpGet("isbn/{isbn:regex(^\\d{{3}}-\\d-\\d{{4}}-\\d{{4}}-\\d$)}")]
    public IActionResult GetByISBN(string isbn)
    {
        _logger.LogInformation("AdvancedRouting: ISBN constraint - {ISBN}", isbn);

        return Ok(new
        {
            Message = $"ISBN {isbn} is valid",
            Route = $"/advanced-routing/isbn/{isbn}",
            Constraint = @"regex(^\d{3}-\d-\d{4}-\d{4}-\d$)",
            Format = "xxx-x-xxxx-xxxx-x",
            ISBN = isbn,
            Example = "/advanced-routing/isbn/978-0-1234-5678-9"
        });
    }

    // ============================================================================
    // OPTIONAL PARAMETERS AND DEFAULT VALUES
    // ============================================================================

    /// <summary>
    /// Optional parameters with default values
    /// Routes: 
    /// - /advanced-routing/search/{query}
    /// - /advanced-routing/search/{query}/page/{page}
    /// - /advanced-routing/search/{query}/page/{page}/size/{size}
    /// </summary>
    [HttpGet("search/{query}/page/{page:int?}/size/{size:int?}")]
    public IActionResult SearchWithOptionalParams(string query, int page = 1, int size = 10)
    {
        _logger.LogInformation("AdvancedRouting: Search with optional params - Query: {Query}, Page: {Page}, Size: {Size}", 
            query, page, size);

        // Validate parameters
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        return Ok(new
        {
            Message = "Search completed with optional parameters",
            Query = query,
            Page = page,
            Size = size,
            Routes = new[]
            {
                $"/advanced-routing/search/{query}",
                $"/advanced-routing/search/{query}/page/{page}",
                $"/advanced-routing/search/{query}/page/{page}/size/{size}"
            },
            Results = Enumerable.Range(1, size).Select(i => new
            {
                Id = (page - 1) * size + i,
                Title = $"Result {i} for '{query}'",
                Page = page
            })
        });
    }

    // ============================================================================
    // CATCH-ALL PARAMETERS
    // ============================================================================

    /// <summary>
    /// Catch-all parameter for file paths
    /// Route: /advanced-routing/files/{*filepath}
    /// Captures everything after /files/ including slashes
    /// </summary>
    [HttpGet("files/{*filepath}")]
    public IActionResult GetFile(string filepath)
    {
        _logger.LogInformation("AdvancedRouting: Catch-all file path - {FilePath}", filepath);

        return Ok(new
        {
            Message = "File path captured",
            Route = "/advanced-routing/files/{*filepath}",
            CapturedPath = filepath,
            Segments = filepath?.Split('/') ?? Array.Empty<string>(),
            Examples = new[]
            {
                "/advanced-routing/files/documents/reports/2023/annual.pdf",
                "/advanced-routing/files/images/gallery/vacation/beach.jpg",
                "/advanced-routing/files/data.csv"
            }
        });
    }

    // ============================================================================
    // ROUTE GENERATION AND URL HELPERS
    // ============================================================================

    /// <summary>
    /// Named route for URL generation
    /// Route: /advanced-routing/products/{id}
    /// </summary>
    [HttpGet("products/{id:int}", Name = "GetProduct")]
    public IActionResult GetProduct(int id)
    {
        _logger.LogInformation("AdvancedRouting: Get product - {ProductId}", id);

        // Generate URLs to other endpoints
        var relatedUrls = new
        {
            Self = Url.Link("GetProduct", new { id }),
            Edit = Url.Action(nameof(EditProduct), new { id }),
            Delete = Url.Action(nameof(DeleteProduct), new { id }),
            Category = Url.Action(nameof(GetProductsByCategory), new { categoryId = 1, id })
        };

        return Ok(new
        {
            Message = $"Product {id} details",
            ProductId = id,
            RouteName = "GetProduct",
            GeneratedUrls = relatedUrls,
            Product = new
            {
                Id = id,
                Name = $"Product {id}",
                Price = 29.99m * id,
                Category = "Electronics"
            }
        });
    }

    /// <summary>
    /// Edit product action for URL generation example
    /// Route: /advanced-routing/products/{id}/edit
    /// </summary>
    [HttpGet("products/{id:int}/edit")]
    public IActionResult EditProduct(int id)
    {
        return Ok(new { Message = $"Edit form for product {id}", ProductId = id });
    }

    /// <summary>
    /// Delete product action for URL generation example
    /// Route: /advanced-routing/products/{id}/delete
    /// </summary>
    [HttpDelete("products/{id:int}/delete")]
    public IActionResult DeleteProduct(int id)
    {
        return Ok(new { Message = $"Product {id} deleted", ProductId = id });
    }

    /// <summary>
    /// Multiple parameter route for URL generation
    /// Route: /advanced-routing/categories/{categoryId}/products/{id}
    /// </summary>
    [HttpGet("categories/{categoryId:int}/products/{id:int}")]
    public IActionResult GetProductsByCategory(int categoryId, int id)
    {
        return Ok(new
        {
            Message = $"Product {id} in category {categoryId}",
            CategoryId = categoryId,
            ProductId = id
        });
    }

    // ============================================================================
    // COMPLEX ROUTING SCENARIOS
    // ============================================================================

    /// <summary>
    /// Multiple constraints on same parameter
    /// Route: /advanced-routing/complex/{value}
    /// Constraint: Must be integer between 10-1000 and divisible by 5
    /// </summary>
    [HttpGet("complex/{value:int:range(10,1000)}")]
    public IActionResult ComplexConstraints(int value)
    {
        // Additional business logic constraint
        if (value % 5 != 0)
        {
            return BadRequest(new
            {
                Error = "Value must be divisible by 5",
                Value = value,
                ValidExamples = new[] { 10, 15, 20, 25, 50, 100 }
            });
        }

        _logger.LogInformation("AdvancedRouting: Complex constraints - {Value}", value);

        return Ok(new
        {
            Message = "Value meets all constraints",
            Route = $"/advanced-routing/complex/{value}",
            Constraints = new[]
            {
                "int (must be integer)",
                "range(10,1000) (must be between 10 and 1000)",
                "divisible by 5 (business rule)"
            },
            Value = value
        });
    }

    /// <summary>
    /// Route with custom model binding
    /// Route: /advanced-routing/coordinates/{lat},{lng}
    /// Custom format for coordinates
    /// </summary>
    [HttpGet("coordinates/{coordinates}")]
    public IActionResult GetByCoordinates(string coordinates)
    {
        _logger.LogInformation("AdvancedRouting: Coordinates - {Coordinates}", coordinates);

        // Parse custom coordinate format: "lat,lng"
        var parts = coordinates.Split(',');
        if (parts.Length != 2 || 
            !decimal.TryParse(parts[0], out var lat) || 
            !decimal.TryParse(parts[1], out var lng))
        {
            return BadRequest(new
            {
                Error = "Invalid coordinate format",
                Expected = "lat,lng (e.g., 40.7128,-74.0060)",
                Received = coordinates
            });
        }

        return Ok(new
        {
            Message = "Coordinates parsed successfully",
            Route = $"/advanced-routing/coordinates/{coordinates}",
            Coordinates = new
            {
                Latitude = lat,
                Longitude = lng,
                Format = "lat,lng"
            },
            Location = new
            {
                City = lat > 40 && lng < -70 ? "New York Area" : "Unknown",
                Hemisphere = new
                {
                    NS = lat >= 0 ? "North" : "South",
                    EW = lng >= 0 ? "East" : "West"
                }
            }
        });
    }

    /// <summary>
    /// Route information and debugging endpoint
    /// Route: /advanced-routing/debug
    /// </summary>
    [HttpGet("debug")]
    public IActionResult GetRoutingDebugInfo()
    {
        var routeData = HttpContext.GetRouteData();

        return Ok(new
        {
            Message = "Routing debug information",
            Request = new
            {
                Method = HttpContext.Request.Method,
                Path = HttpContext.Request.Path.Value,
                QueryString = HttpContext.Request.QueryString.Value,
                Headers = HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            },
            RouteData = new
            {
                Values = routeData.Values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()),
                DataTokens = routeData.DataTokens?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString())
            },
            AvailableRoutes = new[]
            {
                "/advanced-routing/age/{age:int:range(1,120)}",
                "/advanced-routing/username/{username:length(3,20)}",
                "/advanced-routing/guid/{id:guid}",
                "/advanced-routing/date/{date:datetime}",
                "/advanced-routing/price/{price:decimal:min(0.01)}",
                "/advanced-routing/isbn/{isbn:regex pattern}",
                "/advanced-routing/search/{query}/page/{page?}/size/{size?}",
                "/advanced-routing/files/{*filepath}",
                "/advanced-routing/products/{id:int}",
                "/advanced-routing/complex/{value:int:range(10,1000)}",
                "/advanced-routing/coordinates/{coordinates}",
                "/advanced-routing/debug"
            }
        });
    }
}