using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers.V2;

/// <summary>
/// Books API Version 2.0 - Enhanced with pagination, filtering, and detailed information
/// This represents an improved version with advanced features and better data structure
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/books")]
public class BooksV2Controller : ControllerBase
{
    private readonly ILogger<BooksV2Controller> _logger;

    // Enhanced data for V2 with more detailed information
    private static readonly List<BookV2> BooksV2 = new()
    {
        new BookV2 
        { 
            Id = 1, 
            Title = "Foundation", 
            Author = new AuthorInfo { Id = 1, Name = "Isaac Asimov", Nationality = "American" },
            Price = 15.99m, 
            PublishedYear = 1951,
            Genre = "Science Fiction",
            Pages = 244,
            ISBN = "978-0553293357",
            Rating = 4.5m,
            Stock = 50,
            Description = "The first novel in Isaac Asimov's Foundation trilogy"
        },
        new BookV2 
        { 
            Id = 2, 
            Title = "1984", 
            Author = new AuthorInfo { Id = 2, Name = "George Orwell", Nationality = "British" },
            Price = 13.99m, 
            PublishedYear = 1949,
            Genre = "Dystopian Fiction",
            Pages = 328,
            ISBN = "978-0452284234",
            Rating = 4.7m,
            Stock = 75,
            Description = "A dystopian social science fiction novel about totalitarianism"
        },
        new BookV2 
        { 
            Id = 3, 
            Title = "Dune", 
            Author = new AuthorInfo { Id = 3, Name = "Frank Herbert", Nationality = "American" },
            Price = 16.99m, 
            PublishedYear = 1965,
            Genre = "Science Fiction",
            Pages = 688,
            ISBN = "978-0441013593",
            Rating = 4.6m,
            Stock = 30,
            Description = "Epic science fiction novel set on the desert planet Arrakis"
        },
        new BookV2 
        { 
            Id = 4, 
            Title = "The Hobbit", 
            Author = new AuthorInfo { Id = 4, Name = "J.R.R. Tolkien", Nationality = "British" },
            Price = 12.99m, 
            PublishedYear = 1937,
            Genre = "Fantasy",
            Pages = 366,
            ISBN = "978-0547928227",
            Rating = 4.8m,
            Stock = 95,
            Description = "A fantasy adventure novel about Bilbo Baggins"
        },
        new BookV2 
        { 
            Id = 5, 
            Title = "Brave New World", 
            Author = new AuthorInfo { Id = 5, Name = "Aldous Huxley", Nationality = "British" },
            Price = 14.99m, 
            PublishedYear = 1932,
            Genre = "Dystopian Fiction",
            Pages = 288,
            ISBN = "978-0060850524",
            Rating = 4.3m,
            Stock = 40,
            Description = "A dystopian novel exploring themes of technology and society"
        },
        new BookV2 
        { 
            Id = 6, 
            Title = "The Left Hand of Darkness", 
            Author = new AuthorInfo { Id = 6, Name = "Ursula K. Le Guin", Nationality = "American" },
            Price = 15.49m, 
            PublishedYear = 1969,
            Genre = "Science Fiction",
            Pages = 304,
            ISBN = "978-0441478125",
            Rating = 4.4m,
            Stock = 25,
            Description = "Groundbreaking science fiction exploring gender and society"
        }
    };

    public BooksV2Controller(ILogger<BooksV2Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get books with pagination and filtering - V2 (Enhanced with advanced features)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    /// <param name="genre">Filter by genre</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="minRating">Minimum rating filter</param>
    /// <param name="sortBy">Sort field (title, author, price, year, rating)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <returns>Paginated and filtered list of books</returns>
    [HttpGet]
    public ActionResult<PagedResult<BookV2>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? genre = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] string sortBy = "title",
        [FromQuery] string sortOrder = "asc")
    {
        _logger.LogInformation("BooksV2: Getting books with filters - Page: {Page}, Genre: {Genre}, SortBy: {SortBy}", 
            page, genre, sortBy);

        // Validate parameters
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        // Apply filters
        var query = BooksV2.AsQueryable();

        if (!string.IsNullOrEmpty(genre))
            query = query.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

        if (minPrice.HasValue)
            query = query.Where(b => b.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(b => b.Price <= maxPrice.Value);

        if (minRating.HasValue)
            query = query.Where(b => b.Rating >= minRating.Value);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "author" => sortOrder.ToLower() == "desc" 
                ? query.OrderByDescending(b => b.Author.Name)
                : query.OrderBy(b => b.Author.Name),
            "price" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(b => b.Price)
                : query.OrderBy(b => b.Price),
            "year" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(b => b.PublishedYear)
                : query.OrderBy(b => b.PublishedYear),
            "rating" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(b => b.Rating)
                : query.OrderBy(b => b.Rating),
            _ => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var books = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new PagedResult<BookV2>
        {
            Version = "2.0",
            Data = books,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            },
            Filters = new FilterInfo
            {
                Genre = genre,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinRating = minRating,
                SortBy = sortBy,
                SortOrder = sortOrder
            },
            Metadata = new
            {
                Message = "Books retrieved using API Version 2.0",
                Features = new[] 
                { 
                    "Pagination support", 
                    "Genre filtering", 
                    "Price range filtering", 
                    "Rating filtering", 
                    "Multiple sorting options",
                    "Detailed book information"
                }
            }
        };

        return Ok(result);
    }

    /// <summary>
    /// Get book by ID - V2 (Detailed information with related data)
    /// </summary>
    /// <param name="id">Book ID</param>
    /// <returns>Detailed book information</returns>
    [HttpGet("{id:int}")]
    public ActionResult<BookV2> GetBook(int id)
    {
        _logger.LogInformation("BooksV2: Getting detailed book with ID {BookId}", id);

        var book = BooksV2.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound(new
            {
                Version = "2.0",
                Error = new
                {
                    Code = "BOOK_NOT_FOUND",
                    Message = $"Book with ID {id} not found",
                    Details = "The requested book does not exist in our database",
                    Timestamp = DateTime.UtcNow
                },
                Suggestions = new[]
                {
                    "Check the book ID is correct",
                    "Browse all books using GET /api/v2/books",
                    "Search for books using GET /api/v2/books/search"
                }
            });
        }

        // Get related books (same genre)
        var relatedBooks = BooksV2
            .Where(b => b.Id != id && b.Genre == book.Genre)
            .Take(3)
            .Select(b => new { b.Id, b.Title, b.Author.Name, b.Rating })
            .ToList();

        var response = new
        {
            Version = "2.0",
            Message = "Detailed book information retrieved using API Version 2.0",
            Data = book,
            RelatedBooks = relatedBooks,
            Metadata = new
            {
                ViewCount = new Random().Next(100, 1000), // Simulated
                LastUpdated = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                Availability = book.Stock > 0 ? "In Stock" : "Out of Stock",
                EstimatedDelivery = book.Stock > 0 ? "2-3 business days" : "Currently unavailable"
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Advanced search - V2 (Multiple search criteria and options)
    /// </summary>
    /// <param name="query">Search query (title, author, description)</param>
    /// <param name="searchIn">Fields to search in (title, author, description, all)</param>
    /// <param name="exactMatch">Exact match instead of contains</param>
    /// <returns>Search results with relevance scoring</returns>
    [HttpGet("search")]
    public ActionResult<SearchResult<BookV2>> SearchBooks(
        [FromQuery] string query,
        [FromQuery] string searchIn = "all",
        [FromQuery] bool exactMatch = false)
    {
        _logger.LogInformation("BooksV2: Advanced search with query '{Query}', searchIn: {SearchIn}, exactMatch: {ExactMatch}", 
            query, searchIn, exactMatch);

        if (string.IsNullOrEmpty(query))
        {
            return BadRequest(new
            {
                Version = "2.0",
                Error = new
                {
                    Code = "INVALID_SEARCH_QUERY",
                    Message = "Search query is required and cannot be empty",
                    Parameter = "query",
                    Timestamp = DateTime.UtcNow
                },
                Examples = new[]
                {
                    "/api/v2/books/search?query=foundation",
                    "/api/v2/books/search?query=asimov&searchIn=author",
                    "/api/v2/books/search?query=science%20fiction&searchIn=description&exactMatch=true"
                }
            });
        }

        var results = new List<SearchResultItem<BookV2>>();

        foreach (var book in BooksV2)
        {
            var relevanceScore = 0;
            var matchedFields = new List<string>();

            // Search in different fields based on searchIn parameter
            if (searchIn == "all" || searchIn == "title")
            {
                if (SearchMatch(book.Title, query, exactMatch))
                {
                    relevanceScore += 100;
                    matchedFields.Add("title");
                }
            }

            if (searchIn == "all" || searchIn == "author")
            {
                if (SearchMatch(book.Author.Name, query, exactMatch))
                {
                    relevanceScore += 80;
                    matchedFields.Add("author");
                }
            }

            if (searchIn == "all" || searchIn == "description")
            {
                if (SearchMatch(book.Description, query, exactMatch))
                {
                    relevanceScore += 60;
                    matchedFields.Add("description");
                }
            }

            if (searchIn == "all" || searchIn == "genre")
            {
                if (SearchMatch(book.Genre, query, exactMatch))
                {
                    relevanceScore += 40;
                    matchedFields.Add("genre");
                }
            }

            if (relevanceScore > 0)
            {
                results.Add(new SearchResultItem<BookV2>
                {
                    Item = book,
                    RelevanceScore = relevanceScore,
                    MatchedFields = matchedFields
                });
            }
        }

        // Sort by relevance score
        results = results.OrderByDescending(r => r.RelevanceScore).ToList();

        var searchResult = new SearchResult<BookV2>
        {
            Version = "2.0",
            Query = query,
            SearchIn = searchIn,
            ExactMatch = exactMatch,
            Results = results,
            TotalResults = results.Count,
            Metadata = new
            {
                Message = "Advanced search completed using API Version 2.0",
                Features = new[]
                {
                    "Multi-field search",
                    "Relevance scoring",
                    "Exact match option",
                    "Field-specific search",
                    "Matched field indication"
                },
                SearchExecutedAt = DateTime.UtcNow,
                SearchDurationMs = new Random().Next(10, 50) // Simulated
            }
        };

        return Ok(searchResult);
    }

    /// <summary>
    /// Get available genres - V2 (New endpoint)
    /// </summary>
    /// <returns>List of available genres with book counts</returns>
    [HttpGet("genres")]
    public ActionResult<IEnumerable<GenreInfo>> GetGenres()
    {
        _logger.LogInformation("BooksV2: Getting available genres");

        var genres = BooksV2
            .GroupBy(b => b.Genre)
            .Select(g => new GenreInfo
            {
                Name = g.Key,
                BookCount = g.Count(),
                AveragePrice = Math.Round(g.Average(b => b.Price), 2),
                AverageRating = Math.Round(g.Average(b => b.Rating), 1)
            })
            .OrderBy(g => g.Name)
            .ToList();

        return Ok(new
        {
            Version = "2.0",
            Message = "Genres retrieved using API Version 2.0",
            Data = genres,
            TotalGenres = genres.Count,
            Features = new[]
            {
                "Genre statistics",
                "Book count per genre",
                "Average price per genre", 
                "Average rating per genre"
            }
        });
    }

    /// <summary>
    /// Version information endpoint - V2
    /// </summary>
    /// <returns>API version details</returns>
    [HttpGet("version-info")]
    public IActionResult GetVersionInfo()
    {
        _logger.LogInformation("BooksV2: Getting version information");

        var versionInfo = HttpContext.GetRequestedApiVersion();

        return Ok(new
        {
            ApiVersion = "2.0",
            RequestedVersion = versionInfo?.ToString(),
            Description = "Books API Version 2.0 - Enhanced Release",
            Features = new[]
            {
                "Advanced pagination support",
                "Multiple filtering options (genre, price, rating)",
                "Advanced sorting capabilities",
                "Detailed book information with metadata",
                "Multi-field advanced search with relevance scoring",
                "Genre statistics endpoint",
                "Enhanced error handling with structured responses",
                "Related books suggestions",
                "Stock and availability information"
            },
            Improvements = new[]
            {
                "Added pagination for better performance",
                "Enhanced search with relevance scoring",
                "Detailed error responses with suggestions",
                "Rich metadata in responses",
                "Multiple sorting and filtering options",
                "Genre-based book recommendations"
            },
            ReleaseDate = "2024-01-01",
            Endpoints = new[]
            {
                "GET /api/v2/books - With pagination, filtering, and sorting",
                "GET /api/v2/books/{id} - Detailed information with related books",
                "GET /api/v2/books/search - Advanced multi-field search",
                "GET /api/v2/books/genres - Genre statistics",
                "GET /api/v2/books/version-info - Version information"
            },
            BreakingChanges = new[]
            {
                "Response structure changed to include metadata",
                "Pagination is now default behavior",
                "Error responses have structured format",
                "Book model includes additional fields"
            }
        });
    }

    private static bool SearchMatch(string field, string query, bool exactMatch)
    {
        if (string.IsNullOrEmpty(field)) return false;
        
        return exactMatch 
            ? field.Equals(query, StringComparison.OrdinalIgnoreCase)
            : field.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Enhanced book model for API Version 2.0
/// </summary>
public class BookV2
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public AuthorInfo Author { get; set; } = new();
    public decimal Price { get; set; }
    public int PublishedYear { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int Pages { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class AuthorInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public string Version { get; set; } = string.Empty;
    public List<T> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public FilterInfo Filters { get; set; } = new();
    public object? Metadata { get; set; }
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class FilterInfo
{
    public string? Genre { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortOrder { get; set; } = string.Empty;
}

public class SearchResult<T>
{
    public string Version { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string SearchIn { get; set; } = string.Empty;
    public bool ExactMatch { get; set; }
    public List<SearchResultItem<T>> Results { get; set; } = new();
    public int TotalResults { get; set; }
    public object? Metadata { get; set; }
}

public class SearchResultItem<T>
{
    public T Item { get; set; } = default!;
    public int RelevanceScore { get; set; }
    public List<string> MatchedFields { get; set; } = new();
}

public class GenreInfo
{
    public string Name { get; set; } = string.Empty;
    public int BookCount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal AverageRating { get; set; }
}