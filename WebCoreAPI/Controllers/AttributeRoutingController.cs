using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

/// <summary>
/// This controller demonstrates ADVANCED ATTRIBUTE ROUTING
/// Shows various attribute routing patterns, constraints, and techniques
/// </summary>
[ApiController]
public class AttributeRoutingController : ControllerBase
{
    private readonly ILogger<AttributeRoutingController> _logger;

    public AttributeRoutingController(ILogger<AttributeRoutingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic attribute routing
    /// Route: GET /attribute-demo/basic
    /// </summary>
    [HttpGet("attribute-demo/basic")]
    public IActionResult BasicAttributeRouting()
    {
        _logger.LogInformation("AttributeRouting: Basic route");

        return Ok(new
        {
            Message = "Basic attribute routing example",
            Route = "/attribute-demo/basic",
            RoutingType = "Attribute",
            Method = "GET"
        });
    }

    /// <summary>
    /// Route with parameters
    /// Route: GET /books-attr/{id}
    /// </summary>
    [HttpGet("books-attr/{id:int}")]
    public IActionResult GetBookById(int id)
    {
        _logger.LogInformation("AttributeRouting: Get book by ID {BookId}", id);

        return Ok(new
        {
            Message = $"Book {id} retrieved via attribute routing",
            Route = $"/books-attr/{id}",
            RoutingType = "Attribute",
            BookId = id,
            Constraints = "id must be integer"
        });
    }

    /// <summary>
    /// Route with multiple parameters and constraints
    /// Route: GET /books-attr/{categoryId}/author/{authorId}
    /// </summary>
    [HttpGet("books-attr/{categoryId:int:min(1)}/author/{authorId:int:min(1)}")]
    public IActionResult GetBooksByCategoryAndAuthor(int categoryId, int authorId)
    {
        _logger.LogInformation("AttributeRouting: Get books by category {CategoryId} and author {AuthorId}", categoryId, authorId);

        return Ok(new
        {
            Message = "Books filtered by category and author",
            Route = $"/books-attr/{categoryId}/author/{authorId}",
            RoutingType = "Attribute",
            CategoryId = categoryId,
            AuthorId = authorId,
            Constraints = new
            {
                CategoryId = "integer, minimum value 1",
                AuthorId = "integer, minimum value 1"
            }
        });
    }

    /// <summary>
    /// Route with optional parameters
    /// Routes: 
    /// - GET /books-attr/search/{query}
    /// - GET /books-attr/search/{query}/page/{page}
    /// </summary>
    [HttpGet("books-attr/search/{query}/page/{page:int?}")]
    public IActionResult SearchBooksWithPagination(string query, int page = 1)
    {
        _logger.LogInformation("AttributeRouting: Search books '{Query}' on page {Page}", query, page);

        return Ok(new
        {
            Message = $"Search results for '{query}' on page {page}",
            Route = $"/books-attr/search/{query}/page/{page}",
            RoutingType = "Attribute",
            Query = query,
            Page = page,
            OptionalParameter = "page is optional, defaults to 1"
        });
    }

    /// <summary>
    /// Route with string constraints
    /// Route: GET /books-attr/isbn/{isbn}
    /// ISBN format: 3 digits, hyphen, 10 digits
    /// </summary>
    [HttpGet("books-attr/isbn/{isbn:regex(^\\d{{3}}-\\d{{10}}$)}")]
    public IActionResult GetBookByISBN(string isbn)
    {
        _logger.LogInformation("AttributeRouting: Get book by ISBN {ISBN}", isbn);

        return Ok(new
        {
            Message = $"Book with ISBN {isbn}",
            Route = $"/books-attr/isbn/{isbn}",
            RoutingType = "Attribute",
            ISBN = isbn,
            Constraint = "Regex pattern: 3 digits - 10 digits"
        });
    }

    /// <summary>
    /// Multiple HTTP methods on same route
    /// Route: /books-attr/manage/{id}
    /// </summary>
    [HttpGet("books-attr/manage/{id:int}")]
    public IActionResult GetBookForManagement(int id)
    {
        return Ok(new
        {
            Message = $"GET: Managing book {id}",
            Route = $"/books-attr/manage/{id}",
            Method = "GET",
            Action = "Retrieve for management"
        });
    }

    [HttpPut("books-attr/manage/{id:int}")]
    public IActionResult UpdateBookManagement(int id, [FromBody] object data)
    {
        return Ok(new
        {
            Message = $"PUT: Updated book {id}",
            Route = $"/books-attr/manage/{id}",
            Method = "PUT",
            Action = "Update book data",
            Data = data
        });
    }

    [HttpDelete("books-attr/manage/{id:int}")]
    public IActionResult DeleteBookManagement(int id)
    {
        return Ok(new
        {
            Message = $"DELETE: Removed book {id}",
            Route = $"/books-attr/manage/{id}",
            Method = "DELETE",
            Action = "Delete book"
        });
    }

    /// <summary>
    /// Route with multiple route attributes (same action, different routes)
    /// Routes: 
    /// - GET /books-attr/latest
    /// - GET /books-attr/newest
    /// - GET /books-attr/recent
    /// </summary>
    [HttpGet("books-attr/latest")]
    [HttpGet("books-attr/newest")]
    [HttpGet("books-attr/recent")]
    public IActionResult GetLatestBooks()
    {
        _logger.LogInformation("AttributeRouting: Get latest books");

        return Ok(new
        {
            Message = "Latest books retrieved",
            AccessibleVia = new[]
            {
                "/books-attr/latest",
                "/books-attr/newest", 
                "/books-attr/recent"
            },
            RoutingType = "Attribute (Multiple Routes)",
            Books = new[]
            {
                new { Title = "Foundation", Author = "Isaac Asimov", Year = 1951 },
                new { Title = "1984", Author = "George Orwell", Year = 1949 }
            }
        });
    }

    /// <summary>
    /// Route with query string parameters
    /// Route: GET /books-attr/filter?minPrice={minPrice}&maxPrice={maxPrice}&category={category}
    /// </summary>
    [HttpGet("books-attr/filter")]
    public IActionResult FilterBooks(
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? category)
    {
        _logger.LogInformation("AttributeRouting: Filter books - MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, Category: {Category}", 
            minPrice, maxPrice, category);

        return Ok(new
        {
            Message = "Books filtered by query parameters",
            Route = "/books-attr/filter",
            QueryParameters = new
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Category = category
            },
            ExampleUrl = "/books-attr/filter?minPrice=10&maxPrice=20&category=fiction"
        });
    }

    /// <summary>
    /// Route with custom route name (for URL generation)
    /// Route: GET /books-attr/special/{id}
    /// </summary>
    [HttpGet("books-attr/special/{id:int}", Name = "GetSpecialBook")]
    public IActionResult GetSpecialBook(int id)
    {
        _logger.LogInformation("AttributeRouting: Get special book {BookId}", id);

        // Generate URL using route name
        var url = Url.Link("GetSpecialBook", new { id = id });

        return Ok(new
        {
            Message = $"Special book {id}",
            Route = $"/books-attr/special/{id}",
            RouteName = "GetSpecialBook",
            GeneratedUrl = url,
            Usage = "Can be used with Url.Link() for URL generation"
        });
    }

    /// <summary>
    /// Route with version in path
    /// Routes: 
    /// - GET /api/v1/books-attr/version-test
    /// - GET /api/v2/books-attr/version-test
    /// </summary>
    [HttpGet("api/v{version:int}/books-attr/version-test")]
    public IActionResult VersionTest(int version)
    {
        _logger.LogInformation("AttributeRouting: Version {Version} endpoint", version);

        var response = version switch
        {
            1 => new { Message = "Version 1 API", Features = new[] { "Basic CRUD", "Simple search" } },
            2 => new { Message = "Version 2 API", Features = new[] { "Advanced CRUD", "Complex search", "Filtering", "Pagination" } },
            _ => new { Message = $"Version {version} API", Features = new[] { "Unknown version" } }
        };

        return Ok(new
        {
            response.Message,
            Version = version,
            Route = $"/api/v{version}/books-attr/version-test",
            response.Features
        });
    }

    /// <summary>
    /// Route with area-like structure
    /// Route: GET /admin/books-attr/dashboard
    /// </summary>
    [HttpGet("admin/books-attr/dashboard")]
    public IActionResult AdminDashboard()
    {
        _logger.LogInformation("AttributeRouting: Admin dashboard");

        return Ok(new
        {
            Message = "Admin dashboard for books",
            Route = "/admin/books-attr/dashboard",
            Area = "Admin",
            AccessLevel = "Administrator",
            Statistics = new
            {
                TotalBooks = 156,
                ActiveAuthors = 45,
                Categories = 12,
                PendingOrders = 8
            }
        });
    }

    /// <summary>
    /// Catch-all route (must be last)
    /// Route: GET /books-attr/catch-all/{*path}
    /// </summary>
    [HttpGet("books-attr/catch-all/{*path}")]
    public IActionResult CatchAll(string path)
    {
        _logger.LogInformation("AttributeRouting: Catch-all route for path '{Path}'", path);

        return Ok(new
        {
            Message = "Catch-all route activated",
            Route = "/books-attr/catch-all/{*path}",
            CapturedPath = path,
            Purpose = "Handles any unmatched routes under /books-attr/catch-all/",
            Example = "Try /books-attr/catch-all/some/nested/path"
        });
    }
}