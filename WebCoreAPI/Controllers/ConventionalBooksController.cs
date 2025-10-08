using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

/// <summary>
/// This controller demonstrates CONVENTIONAL ROUTING
/// Routes are defined in Program.cs using MapControllerRoute
/// </summary>
public class ConventionalBooksController : ControllerBase
{
    private readonly ILogger<ConventionalBooksController> _logger;

    // Using the same static data as BooksController for consistency
    private static readonly List<Book> Books = GetSampleBooks();
    private static readonly List<Author> Authors = GetSampleAuthors();
    private static readonly List<Category> Categories = GetSampleCategories();

    public ConventionalBooksController(ILogger<ConventionalBooksController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Default action - accessible via:
    /// - /api/ConventionalBooks
    /// - /bookstore/ConventionalBooks
    /// </summary>
    public IActionResult Get()
    {
        _logger.LogInformation("ConventionalBooks: Get all books");
        
        var result = new
        {
            Message = "Books retrieved via CONVENTIONAL ROUTING",
            RoutingType = "Conventional",
            AccessibleVia = new[]
            {
                "/api/ConventionalBooks",
                "/api/ConventionalBooks/Get", 
                "/bookstore/ConventionalBooks",
                "/bookstore/ConventionalBooks/Get"
            },
            Books = Books.Take(3).Select(b => new { b.Id, b.Title, b.Author.FullName, b.Price })
        };

        return Ok(result);
    }

    /// <summary>
    /// Get book by ID - accessible via:
    /// - /api/ConventionalBooks/Get/1
    /// - /bookstore/ConventionalBooks/Get/1
    /// </summary>
    public IActionResult Get(int id)
    {
        _logger.LogInformation("ConventionalBooks: Get book by ID {BookId}", id);
        
        var book = Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found");
        }

        var result = new
        {
            Message = $"Book {id} retrieved via CONVENTIONAL ROUTING",
            RoutingType = "Conventional",
            AccessibleVia = new[]
            {
                $"/api/ConventionalBooks/Get/{id}",
                $"/bookstore/ConventionalBooks/Get/{id}"
            },
            Book = new { book.Id, book.Title, book.Author.FullName, book.Price, book.ISBN }
        };

        return Ok(result);
    }

    /// <summary>
    /// Get books by category - accessible via:
    /// - /categories/1/books (uses custom conventional route)
    /// - /categories/1/books/GetByCategory
    /// </summary>
    public IActionResult GetByCategory(int categoryId)
    {
        _logger.LogInformation("ConventionalBooks: Get books by category {CategoryId}", categoryId);

        var category = Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category == null)
        {
            return NotFound($"Category with ID {categoryId} not found");
        }

        var categoryBooks = Books.Where(b => b.CategoryId == categoryId).ToList();

        var result = new
        {
            Message = $"Books in category '{category.Name}' retrieved via CONVENTIONAL ROUTING",
            RoutingType = "Conventional",
            RoutePattern = "categories/{categoryId:int}/books/{action=GetByCategory}",
            AccessibleVia = new[]
            {
                $"/categories/{categoryId}/books",
                $"/categories/{categoryId}/books/GetByCategory"
            },
            Category = new { category.Id, category.Name },
            Books = categoryBooks.Select(b => new { b.Id, b.Title, b.Author.FullName, b.Price })
        };

        return Ok(result);
    }

    /// <summary>
    /// Get books by author - accessible via:
    /// - /authors/1/books (uses custom conventional route)
    /// - /authors/1/books/GetByAuthor
    /// </summary>
    public IActionResult GetByAuthor(int authorId)
    {
        _logger.LogInformation("ConventionalBooks: Get books by author {AuthorId}", authorId);

        var author = Authors.FirstOrDefault(a => a.Id == authorId);
        if (author == null)
        {
            return NotFound($"Author with ID {authorId} not found");
        }

        var authorBooks = Books.Where(b => b.AuthorId == authorId).ToList();

        var result = new
        {
            Message = $"Books by '{author.FullName}' retrieved via CONVENTIONAL ROUTING",
            RoutingType = "Conventional", 
            RoutePattern = "authors/{authorId:int}/books/{action=GetByAuthor}",
            AccessibleVia = new[]
            {
                $"/authors/{authorId}/books",
                $"/authors/{authorId}/books/GetByAuthor"
            },
            Author = new { author.Id, author.FullName },
            Books = authorBooks.Select(b => new { b.Id, b.Title, b.Price, b.PublishedDate })
        };

        return Ok(result);
    }

    /// <summary>
    /// Search books - accessible via:
    /// - /api/ConventionalBooks/Search?query=foundation
    /// - /bookstore/ConventionalBooks/Search?query=foundation
    /// </summary>
    public IActionResult Search(string query)
    {
        _logger.LogInformation("ConventionalBooks: Search books with query '{Query}'", query);

        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query cannot be empty");
        }

        var searchResults = Books.Where(b => 
            b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            b.Author.FullName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var result = new
        {
            Message = $"Search results for '{query}' via CONVENTIONAL ROUTING",
            RoutingType = "Conventional",
            Query = query,
            AccessibleVia = new[]
            {
                $"/api/ConventionalBooks/Search?query={query}",
                $"/bookstore/ConventionalBooks/Search?query={query}"
            },
            ResultCount = searchResults.Count,
            Books = searchResults.Select(b => new { b.Id, b.Title, b.Author.FullName, b.Price })
        };

        return Ok(result);
    }

    /// <summary>
    /// Get popular books - accessible via:
    /// - /api/ConventionalBooks/Popular
    /// - /bookstore/ConventionalBooks/Popular  
    /// </summary>
    public IActionResult Popular()
    {
        _logger.LogInformation("ConventionalBooks: Get popular books");

        // Simulate popular books (highest stock quantity)
        var popularBooks = Books.OrderByDescending(b => b.StockQuantity).Take(3).ToList();

        var result = new
        {
            Message = "Popular books retrieved via CONVENTIONAL ROUTING",
            RoutingType = "Conventional",
            AccessibleVia = new[]
            {
                "/api/ConventionalBooks/Popular",
                "/bookstore/ConventionalBooks/Popular"
            },
            Books = popularBooks.Select(b => new { 
                b.Id, 
                b.Title, 
                b.Author.FullName, 
                b.Price, 
                b.StockQuantity,
                PopularityScore = b.StockQuantity // Simple popularity metric
            })
        };

        return Ok(result);
    }

    // Helper methods to get sample data
    private static List<Book> GetSampleBooks()
    {
        var authors = GetSampleAuthors();
        var categories = GetSampleCategories();

        return new List<Book>
        {
            new Book { Id = 1, Title = "Foundation", ISBN = "978-0553293357", Price = 15.99m, StockQuantity = 50, PublishedDate = new DateTime(1951, 5, 1), Pages = 244, Publisher = "Gnome Press", AuthorId = 1, CategoryId = 3, Author = authors[0], Category = categories[2] },
            new Book { Id = 2, Title = "Murder on the Orient Express", ISBN = "978-0062693662", Price = 12.99m, StockQuantity = 30, PublishedDate = new DateTime(1934, 1, 1), Pages = 256, Publisher = "Collins Crime Club", AuthorId = 2, CategoryId = 4, Author = authors[1], Category = categories[3] },
            new Book { Id = 3, Title = "The Shining", ISBN = "978-0307743657", Price = 14.99m, StockQuantity = 25, PublishedDate = new DateTime(1977, 1, 28), Pages = 447, Publisher = "Doubleday", AuthorId = 3, CategoryId = 1, Author = authors[2], Category = categories[0] },
            new Book { Id = 4, Title = "Harry Potter and the Philosopher's Stone", ISBN = "978-0747532699", Price = 18.99m, StockQuantity = 100, PublishedDate = new DateTime(1997, 6, 26), Pages = 223, Publisher = "Bloomsbury", AuthorId = 4, CategoryId = 1, Author = authors[3], Category = categories[0] },
            new Book { Id = 5, Title = "1984", ISBN = "978-0452284234", Price = 13.99m, StockQuantity = 75, PublishedDate = new DateTime(1949, 6, 8), Pages = 328, Publisher = "Secker & Warburg", AuthorId = 5, CategoryId = 3, Author = authors[4], Category = categories[2] }
        };
    }

    private static List<Author> GetSampleAuthors()
    {
        return new List<Author>
        {
            new Author { Id = 1, FirstName = "Isaac", LastName = "Asimov", Email = "isaac@example.com", Nationality = "American", DateOfBirth = new DateTime(1920, 1, 2) },
            new Author { Id = 2, FirstName = "Agatha", LastName = "Christie", Email = "agatha@example.com", Nationality = "British", DateOfBirth = new DateTime(1890, 9, 15) },
            new Author { Id = 3, FirstName = "Stephen", LastName = "King", Email = "stephen@example.com", Nationality = "American", DateOfBirth = new DateTime(1947, 9, 21) },
            new Author { Id = 4, FirstName = "J.K.", LastName = "Rowling", Email = "jk@example.com", Nationality = "British", DateOfBirth = new DateTime(1965, 7, 31) },
            new Author { Id = 5, FirstName = "George", LastName = "Orwell", Email = "george@example.com", Nationality = "British", DateOfBirth = new DateTime(1903, 6, 25) }
        };
    }

    private static List<Category> GetSampleCategories()
    {
        return new List<Category>
        {
            new Category { Id = 1, Name = "Fiction", Description = "Fictional literature and novels" },
            new Category { Id = 2, Name = "Non-Fiction", Description = "Factual and informative books" },
            new Category { Id = 3, Name = "Science Fiction", Description = "Futuristic and speculative fiction" },
            new Category { Id = 4, Name = "Mystery", Description = "Detective and mystery novels" },
            new Category { Id = 5, Name = "Biography", Description = "Life stories and memoirs" }
        };
    }
}