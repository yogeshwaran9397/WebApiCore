using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers.V1;

/// <summary>
/// Books API Version 1.0 - Basic CRUD operations
/// This represents the initial version of the Books API with simple functionality
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/books")]
[Route("api/books")] // Fallback route for non-versioned requests
public class BooksV1Controller : ControllerBase
{
    private readonly ILogger<BooksV1Controller> _logger;

    // Simple in-memory data for V1
    private static readonly List<BookV1> BooksV1 = new()
    {
        new BookV1 { Id = 1, Title = "Foundation", Author = "Isaac Asimov", Price = 15.99m, Year = 1951 },
        new BookV1 { Id = 2, Title = "1984", Author = "George Orwell", Price = 13.99m, Year = 1949 },
        new BookV1 { Id = 3, Title = "Dune", Author = "Frank Herbert", Price = 16.99m, Year = 1965 },
        new BookV1 { Id = 4, Title = "The Hobbit", Author = "J.R.R. Tolkien", Price = 12.99m, Year = 1937 },
        new BookV1 { Id = 5, Title = "Brave New World", Author = "Aldous Huxley", Price = 14.99m, Year = 1932 }
    };

    public BooksV1Controller(ILogger<BooksV1Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all books - V1 (Simple list with basic information)
    /// </summary>
    /// <returns>List of books with basic information</returns>
    [HttpGet]
    public ActionResult<IEnumerable<BookV1>> GetBooks()
    {
        _logger.LogInformation("BooksV1: Getting all books");

        var response = new
        {
            Version = "1.0",
            Message = "Books retrieved using API Version 1.0",
            Features = new[] { "Basic book listing", "Simple data structure", "No pagination" },
            Data = BooksV1,
            Count = BooksV1.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// Get book by ID - V1 (Basic information only)
    /// </summary>
    /// <param name="id">Book ID</param>
    /// <returns>Book details</returns>
    [HttpGet("{id:int}")]
    public ActionResult<BookV1> GetBook(int id)
    {
        _logger.LogInformation("BooksV1: Getting book with ID {BookId}", id);

        var book = BooksV1.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound(new
            {
                Version = "1.0",
                Error = $"Book with ID {id} not found",
                Message = "This is API Version 1.0 - limited error information"
            });
        }

        var response = new
        {
            Version = "1.0",
            Message = "Book retrieved using API Version 1.0",
            Data = book
        };

        return Ok(response);
    }

    /// <summary>
    /// Search books - V1 (Simple title search only)
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>Matching books</returns>
    [HttpGet("search")]
    public ActionResult<IEnumerable<BookV1>> SearchBooks([FromQuery] string query)
    {
        _logger.LogInformation("BooksV1: Searching books with query '{Query}'", query);

        if (string.IsNullOrEmpty(query))
        {
            return BadRequest(new
            {
                Version = "1.0",
                Error = "Search query is required",
                Message = "API Version 1.0 - basic validation only"
            });
        }

        var results = BooksV1.Where(b => 
            b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var response = new
        {
            Version = "1.0",
            Message = "Search completed using API Version 1.0",
            Features = new[] { "Title and author search", "Case-insensitive", "No advanced filtering" },
            Query = query,
            Results = results,
            Count = results.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// Create book - V1 (Basic creation)
    /// </summary>
    /// <param name="book">Book to create</param>
    /// <returns>Created book</returns>
    [HttpPost]
    public ActionResult<BookV1> CreateBook([FromBody] CreateBookV1Request book)
    {
        _logger.LogInformation("BooksV1: Creating book '{Title}'", book.Title);

        var newBook = new BookV1
        {
            Id = BooksV1.Max(b => b.Id) + 1,
            Title = book.Title,
            Author = book.Author,
            Price = book.Price,
            Year = book.Year
        };

        BooksV1.Add(newBook);

        var response = new
        {
            Version = "1.0",
            Message = "Book created using API Version 1.0",
            Features = new[] { "Basic creation", "No validation", "Simple data structure" },
            Data = newBook
        };

        return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, response);
    }

    /// <summary>
    /// Version information endpoint
    /// </summary>
    /// <returns>API version details</returns>
    [HttpGet("version-info")]
    public IActionResult GetVersionInfo()
    {
        _logger.LogInformation("BooksV1: Getting version information");

        var versionInfo = HttpContext.GetRequestedApiVersion();

        return Ok(new
        {
            ApiVersion = "1.0",
            RequestedVersion = versionInfo?.ToString(),
            Description = "Books API Version 1.0 - Initial Release",
            Features = new[]
            {
                "Basic CRUD operations",
                "Simple book model",
                "Title and author search",
                "No pagination",
                "No advanced filtering",
                "Basic error handling"
            },
            Limitations = new[]
            {
                "No pagination support",
                "Limited search capabilities",
                "No data validation",
                "Simple error responses",
                "No relationship data"
            },
            ReleaseDate = "2023-01-01",
            Endpoints = new[]
            {
                "GET /api/v1/books",
                "GET /api/v1/books/{id}",
                "GET /api/v1/books/search?query={query}",
                "POST /api/v1/books",
                "GET /api/v1/books/version-info"
            }
        });
    }
}

/// <summary>
/// Simple book model for API Version 1.0
/// </summary>
public class BookV1
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Year { get; set; }
}

/// <summary>
/// Request model for creating books in V1
/// </summary>
public class CreateBookV1Request
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Year { get; set; }
}