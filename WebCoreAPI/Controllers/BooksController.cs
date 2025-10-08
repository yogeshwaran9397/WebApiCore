using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class BooksController : ControllerBase
{
    private readonly ILogger<BooksController> _logger;
    
    // In-memory storage for demo purposes
    private static readonly List<Book> Books = new();
    private static readonly List<Author> Authors = new();
    private static readonly List<Category> Categories = new();
    private static int _nextBookId = 1;

    static BooksController()
    {
        SeedData();
    }

    public BooksController(ILogger<BooksController> logger)
    {
        _logger = logger;
    }

    // GET: api/books
    [HttpGet]
    public ActionResult<IEnumerable<Book>> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Getting books with pagination: page {Page}, pageSize {PageSize}", page, pageSize);
        
        var skip = (page - 1) * pageSize;
        var books = Books.Skip(skip).Take(pageSize).ToList();
        
        var response = new
        {
            Data = books,
            Page = page,
            PageSize = pageSize,
            TotalCount = Books.Count,
            TotalPages = (int)Math.Ceiling((double)Books.Count / pageSize)
        };

        return Ok(response);
    }

    // GET: api/books/{id}
    [HttpGet("{id}")]
    public ActionResult<Book> GetBook(int id)
    {
        _logger.LogInformation("Getting book with ID: {BookId}", id);
        
        var book = Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found");
        }

        return Ok(book);
    }

    // POST: api/books
    [HttpPost]
    public ActionResult<Book> CreateBook([FromBody] CreateBookRequest request)
    {
        _logger.LogInformation("Creating new book: {Title}", request.Title);

        var author = Authors.FirstOrDefault(a => a.Id == request.AuthorId);
        var category = Categories.FirstOrDefault(c => c.Id == request.CategoryId);

        if (author == null)
            return BadRequest($"Author with ID {request.AuthorId} not found");
        
        if (category == null)
            return BadRequest($"Category with ID {request.CategoryId} not found");

        var book = new Book
        {
            Id = _nextBookId++,
            Title = request.Title,
            ISBN = request.ISBN,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            PublishedDate = request.PublishedDate,
            Pages = request.Pages,
            Language = request.Language,
            Publisher = request.Publisher,
            AuthorId = request.AuthorId,
            CategoryId = request.CategoryId,
            Author = author,
            Category = category
        };

        Books.Add(book);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // PUT: api/books/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateBook(int id, [FromBody] CreateBookRequest request)
    {
        _logger.LogInformation("Updating book with ID: {BookId}", id);
        
        var book = Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found");
        }

        var author = Authors.FirstOrDefault(a => a.Id == request.AuthorId);
        var category = Categories.FirstOrDefault(c => c.Id == request.CategoryId);

        if (author == null)
            return BadRequest($"Author with ID {request.AuthorId} not found");
        
        if (category == null)
            return BadRequest($"Category with ID {request.CategoryId} not found");

        book.Title = request.Title;
        book.ISBN = request.ISBN;
        book.Description = request.Description;
        book.Price = request.Price;
        book.StockQuantity = request.StockQuantity;
        book.PublishedDate = request.PublishedDate;
        book.Pages = request.Pages;
        book.Language = request.Language;
        book.Publisher = request.Publisher;
        book.AuthorId = request.AuthorId;
        book.CategoryId = request.CategoryId;
        book.Author = author;
        book.Category = category;
        book.UpdatedAt = DateTime.UtcNow;

        return NoContent();
    }

    // DELETE: api/books/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteBook(int id)
    {
        _logger.LogInformation("Deleting book with ID: {BookId}", id);
        
        var book = Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found");
        }

        Books.Remove(book);
        return NoContent();
    }

    // GET: api/books/search
    [HttpGet("search")]
    public ActionResult<IEnumerable<Book>> SearchBooks([FromQuery] string? title, [FromQuery] string? author, [FromQuery] string? isbn)
    {
        _logger.LogInformation("Searching books with title: {Title}, author: {Author}, ISBN: {ISBN}", title, author, isbn);
        
        var query = Books.AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(author))
            query = query.Where(b => b.Author.FullName.Contains(author, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(isbn))
            query = query.Where(b => b.ISBN.Contains(isbn, StringComparison.OrdinalIgnoreCase));

        return Ok(query.ToList());
    }

    private static void SeedData()
    {
        // Seed Categories
        Categories.AddRange(new[]
        {
            new Category { Id = 1, Name = "Fiction", Description = "Fictional literature and novels" },
            new Category { Id = 2, Name = "Non-Fiction", Description = "Factual and informative books" },
            new Category { Id = 3, Name = "Science Fiction", Description = "Futuristic and speculative fiction" },
            new Category { Id = 4, Name = "Mystery", Description = "Detective and mystery novels" },
            new Category { Id = 5, Name = "Biography", Description = "Life stories and memoirs" }
        });

        // Seed Authors
        Authors.AddRange(new[]
        {
            new Author { Id = 1, FirstName = "Isaac", LastName = "Asimov", Email = "isaac@example.com", Nationality = "American", DateOfBirth = new DateTime(1920, 1, 2) },
            new Author { Id = 2, FirstName = "Agatha", LastName = "Christie", Email = "agatha@example.com", Nationality = "British", DateOfBirth = new DateTime(1890, 9, 15) },
            new Author { Id = 3, FirstName = "Stephen", LastName = "King", Email = "stephen@example.com", Nationality = "American", DateOfBirth = new DateTime(1947, 9, 21) },
            new Author { Id = 4, FirstName = "J.K.", LastName = "Rowling", Email = "jk@example.com", Nationality = "British", DateOfBirth = new DateTime(1965, 7, 31) },
            new Author { Id = 5, FirstName = "George", LastName = "Orwell", Email = "george@example.com", Nationality = "British", DateOfBirth = new DateTime(1903, 6, 25) }
        });

        // Seed Books
        Books.AddRange(new[]
        {
            new Book { Id = 1, Title = "Foundation", ISBN = "978-0553293357", Description = "A science fiction novel about psychohistory", Price = 15.99m, StockQuantity = 50, PublishedDate = new DateTime(1951, 5, 1), Pages = 244, Publisher = "Gnome Press", AuthorId = 1, CategoryId = 3, Author = Authors[0], Category = Categories[2] },
            new Book { Id = 2, Title = "Murder on the Orient Express", ISBN = "978-0062693662", Description = "A mystery novel featuring Hercule Poirot", Price = 12.99m, StockQuantity = 30, PublishedDate = new DateTime(1934, 1, 1), Pages = 256, Publisher = "Collins Crime Club", AuthorId = 2, CategoryId = 4, Author = Authors[1], Category = Categories[3] },
            new Book { Id = 3, Title = "The Shining", ISBN = "978-0307743657", Description = "A horror novel about the Overlook Hotel", Price = 14.99m, StockQuantity = 25, PublishedDate = new DateTime(1977, 1, 28), Pages = 447, Publisher = "Doubleday", AuthorId = 3, CategoryId = 1, Author = Authors[2], Category = Categories[0] },
            new Book { Id = 4, Title = "Harry Potter and the Philosopher's Stone", ISBN = "978-0747532699", Description = "The first book in the Harry Potter series", Price = 18.99m, StockQuantity = 100, PublishedDate = new DateTime(1997, 6, 26), Pages = 223, Publisher = "Bloomsbury", AuthorId = 4, CategoryId = 1, Author = Authors[3], Category = Categories[0] },
            new Book { Id = 5, Title = "1984", ISBN = "978-0452284234", Description = "A dystopian social science fiction novel", Price = 13.99m, StockQuantity = 75, PublishedDate = new DateTime(1949, 6, 8), Pages = 328, Publisher = "Secker & Warburg", AuthorId = 5, CategoryId = 3, Author = Authors[4], Category = Categories[2] }
        });

        _nextBookId = 6;
    }
}

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime PublishedDate { get; set; }
    public int Pages { get; set; }
    public string Language { get; set; } = "English";
    public string? Publisher { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
}