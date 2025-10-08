using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

/// <summary>
/// This controller demonstrates HYBRID ROUTING - mixing conventional and attribute routing
/// Some actions use conventional routing, others use attribute routing
/// </summary>
[Route("hybrid")] // Base route for attribute routing actions
public class HybridRoutingController : ControllerBase
{
    private readonly ILogger<HybridRoutingController> _logger;

    public HybridRoutingController(ILogger<HybridRoutingController> logger)
    {
        _logger = logger;
    }

    // ============================================================================
    // CONVENTIONAL ROUTING ACTIONS
    // These actions can be accessed via conventional routes defined in Program.cs
    // ============================================================================

    /// <summary>
    /// CONVENTIONAL ROUTING: Default Index action
    /// Accessible via: /api/HybridRouting, /api/HybridRouting/Index
    /// </summary>
    public IActionResult Index()
    {
        _logger.LogInformation("HybridRouting: Index action via conventional routing");

        return Ok(new
        {
            Message = "Welcome to Hybrid Routing Controller",
            Action = "Index",
            RoutingType = "Conventional",
            AccessibleVia = new[]
            {
                "/api/HybridRouting",
                "/api/HybridRouting/Index"
            },
            Description = "This action uses conventional routing defined in Program.cs"
        });
    }

    /// <summary>
    /// CONVENTIONAL ROUTING: Get all items
    /// Accessible via: /api/HybridRouting/GetAll
    /// </summary>
    public IActionResult GetAll()
    {
        _logger.LogInformation("HybridRouting: GetAll action via conventional routing");

        return Ok(new
        {
            Message = "All items retrieved",
            Action = "GetAll", 
            RoutingType = "Conventional",
            AccessibleVia = "/api/HybridRouting/GetAll",
            Items = new[]
            {
                new { Id = 1, Name = "Item 1", Type = "Conventional" },
                new { Id = 2, Name = "Item 2", Type = "Conventional" },
                new { Id = 3, Name = "Item 3", Type = "Conventional" }
            }
        });
    }

    /// <summary>
    /// CONVENTIONAL ROUTING: Get item by ID 
    /// Accessible via: /api/HybridRouting/GetById/123
    /// </summary>
    public IActionResult GetById(int id)
    {
        _logger.LogInformation("HybridRouting: GetById action via conventional routing for ID {Id}", id);

        return Ok(new
        {
            Message = $"Item {id} retrieved",
            Action = "GetById",
            RoutingType = "Conventional",
            AccessibleVia = $"/api/HybridRouting/GetById/{id}",
            Item = new { Id = id, Name = $"Item {id}", Type = "Conventional" }
        });
    }

    // ============================================================================
    // ATTRIBUTE ROUTING ACTIONS  
    // These actions use attribute routing with the [Route] attribute
    // ============================================================================

    /// <summary>
    /// ATTRIBUTE ROUTING: Custom attribute route
    /// Accessible via: /hybrid/custom-endpoint
    /// </summary>
    [HttpGet("custom-endpoint")]
    public IActionResult CustomEndpoint()
    {
        _logger.LogInformation("HybridRouting: CustomEndpoint action via attribute routing");

        return Ok(new
        {
            Message = "Custom endpoint accessed",
            Action = "CustomEndpoint",
            RoutingType = "Attribute", 
            AccessibleVia = "/hybrid/custom-endpoint",
            Description = "This action uses [HttpGet] attribute routing"
        });
    }

    /// <summary>
    /// ATTRIBUTE ROUTING: Route with parameters
    /// Accessible via: /hybrid/items/{id}
    /// </summary>
    [HttpGet("items/{id:int}")]
    public IActionResult GetItemAttribute(int id)
    {
        _logger.LogInformation("HybridRouting: GetItemAttribute action via attribute routing for ID {Id}", id);

        return Ok(new
        {
            Message = $"Item {id} retrieved via attribute routing",
            Action = "GetItemAttribute",
            RoutingType = "Attribute",
            AccessibleVia = $"/hybrid/items/{id}",
            Item = new { Id = id, Name = $"Attribute Item {id}", Type = "Attribute" }
        });
    }

    /// <summary>
    /// ATTRIBUTE ROUTING: POST action with complex route
    /// Accessible via: POST /hybrid/categories/{categoryId}/items
    /// </summary>
    [HttpPost("categories/{categoryId:int}/items")]
    public IActionResult CreateItemInCategory(int categoryId, [FromBody] CreateItemRequest request)
    {
        _logger.LogInformation("HybridRouting: CreateItemInCategory via attribute routing for category {CategoryId}", categoryId);

        return Ok(new
        {
            Message = $"Item '{request.Name}' created in category {categoryId}",
            Action = "CreateItemInCategory",
            RoutingType = "Attribute",
            AccessibleVia = $"/hybrid/categories/{categoryId}/items",
            Method = "POST",
            CategoryId = categoryId,
            CreatedItem = new 
            { 
                Id = new Random().Next(100, 999),
                Name = request.Name,
                Description = request.Description,
                CategoryId = categoryId,
                Type = "Attribute Created"
            }
        });
    }

    /// <summary>
    /// ATTRIBUTE ROUTING: Multiple route attributes on same action
    /// Accessible via: 
    /// - /hybrid/search
    /// - /hybrid/find  
    /// - /hybrid/lookup
    /// </summary>
    [HttpGet("search")]
    [HttpGet("find")]
    [HttpGet("lookup")]
    public IActionResult SearchItems([FromQuery] string query)
    {
        _logger.LogInformation("HybridRouting: SearchItems via attribute routing with query '{Query}'", query);

        return Ok(new
        {
            Message = $"Search results for '{query}'",
            Action = "SearchItems",
            RoutingType = "Attribute (Multiple Routes)",
            Query = query,
            AccessibleVia = new[]
            {
                $"/hybrid/search?query={query}",
                $"/hybrid/find?query={query}",
                $"/hybrid/lookup?query={query}"
            },
            Results = new[]
            {
                new { Id = 1, Name = $"Result 1 for {query}", Relevance = 95 },
                new { Id = 2, Name = $"Result 2 for {query}", Relevance = 87 },
                new { Id = 3, Name = $"Result 3 for {query}", Relevance = 76 }
            }
        });
    }

    // ============================================================================
    // MIXED ROUTING DEMONSTRATION
    // Actions that show how both routing types can coexist
    // ============================================================================

    /// <summary>
    /// CONVENTIONAL + ATTRIBUTE: This action can be accessed via both routing types
    /// Conventional: /api/HybridRouting/CompareRouting  
    /// Attribute: /hybrid/compare (defined below)
    /// </summary>
    public IActionResult CompareRouting()
    {
        _logger.LogInformation("HybridRouting: CompareRouting action (accessible via both routing types)");

        return Ok(new
        {
            Message = "Action accessible via both conventional and attribute routing",
            Action = "CompareRouting",
            RoutingTypes = "Both Conventional and Attribute",
            ConventionalRoute = "/api/HybridRouting/CompareRouting",
            AttributeRoute = "/hybrid/compare",
            Description = "Same action, multiple routing approaches"
        });
    }

    /// <summary>
    /// ATTRIBUTE ROUTING: Alternative route for the same action above
    /// Accessible via: /hybrid/compare
    /// </summary>
    [HttpGet("compare")]
    public IActionResult CompareRoutingAttribute()
    {
        // Call the same logic as CompareRouting but indicate it came from attribute route
        var result = CompareRouting() as OkObjectResult;
        var data = result?.Value as dynamic;

        return Ok(new
        {
            Message = "Same action logic, but accessed via attribute routing",
            Action = "CompareRoutingAttribute", 
            RoutingType = "Attribute",
            AccessibleVia = "/hybrid/compare",
            OriginalAction = "CompareRouting",
            Note = "This demonstrates how to have multiple routes to similar functionality"
        });
    }

    /// <summary>
    /// ROUTE INFORMATION: Shows routing information for debugging
    /// Accessible via: /hybrid/route-info
    /// </summary>
    [HttpGet("route-info")]
    public IActionResult GetRouteInfo()
    {
        _logger.LogInformation("HybridRouting: GetRouteInfo action");

        var routeData = HttpContext.GetRouteData();
        var routeValues = routeData.Values;

        return Ok(new
        {
            Message = "Current route information",
            Action = "GetRouteInfo",
            RoutingType = "Attribute",
            AccessibleVia = "/hybrid/route-info",
            RouteData = new
            {
                Controller = routeValues["controller"]?.ToString(),
                Action = routeValues["action"]?.ToString(),
                AllValues = routeValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString())
            },
            RequestInfo = new
            {
                Method = HttpContext.Request.Method,
                Path = HttpContext.Request.Path.Value,
                QueryString = HttpContext.Request.QueryString.Value
            }
        });
    }
}

/// <summary>
/// Request model for creating items
/// </summary>
public class CreateItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}