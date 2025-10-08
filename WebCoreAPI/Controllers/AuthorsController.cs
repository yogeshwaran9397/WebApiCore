using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class AuthorsController : ControllerBase
{
    private readonly ILogger<AuthorsController> _logger;
    
    // In-memory storage for demo purposes
    private static readonly List<Author> Authors = new();
    private static int _nextAuthorId = 1;

    static AuthorsController()
    {
        SeedData();
    }

    public AuthorsController(ILogger<AuthorsController> logger)
    {
        _logger = logger;
    }

    // GET: api/authors
    [HttpGet]
    public ActionResult<IEnumerable<Author>> GetAuthors()
    {
        _logger.LogInformation("Getting all authors");
        return Ok(Authors);
    }

    // GET: api/authors/{id}
    [HttpGet("{id}")]
    public ActionResult<Author> GetAuthor(int id)
    {
        _logger.LogInformation("Getting author with ID: {AuthorId}", id);
        
        var author = Authors.FirstOrDefault(a => a.Id == id);
        if (author == null)
        {
            return NotFound($"Author with ID {id} not found");
        }

        return Ok(author);
    }

    // POST: api/authors
    [HttpPost]
    public ActionResult<Author> CreateAuthor([FromBody] CreateAuthorRequest request)
    {
        _logger.LogInformation("Creating new author: {FirstName} {LastName}", request.FirstName, request.LastName);

        var author = new Author
        {
            Id = _nextAuthorId++,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Biography = request.Biography,
            DateOfBirth = request.DateOfBirth,
            Nationality = request.Nationality
        };

        Authors.Add(author);
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    // PUT: api/authors/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateAuthor(int id, [FromBody] CreateAuthorRequest request)
    {
        _logger.LogInformation("Updating author with ID: {AuthorId}", id);
        
        var author = Authors.FirstOrDefault(a => a.Id == id);
        if (author == null)
        {
            return NotFound($"Author with ID {id} not found");
        }

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Email = request.Email;
        author.Biography = request.Biography;
        author.DateOfBirth = request.DateOfBirth;
        author.Nationality = request.Nationality;
        author.UpdatedAt = DateTime.UtcNow;

        return NoContent();
    }

    // DELETE: api/authors/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteAuthor(int id)
    {
        _logger.LogInformation("Deleting author with ID: {AuthorId}", id);
        
        var author = Authors.FirstOrDefault(a => a.Id == id);
        if (author == null)
        {
            return NotFound($"Author with ID {id} not found");
        }

        Authors.Remove(author);
        return NoContent();
    }

    // GET: api/authors/search
    [HttpGet("search")]
    public ActionResult<IEnumerable<Author>> SearchAuthors([FromQuery] string? name, [FromQuery] string? nationality)
    {
        _logger.LogInformation("Searching authors with name: {Name}, nationality: {Nationality}", name, nationality);
        
        var query = Authors.AsQueryable();

        if (!string.IsNullOrEmpty(name))
            query = query.Where(a => a.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(nationality))
            query = query.Where(a => a.Nationality != null && a.Nationality.Contains(nationality, StringComparison.OrdinalIgnoreCase));

        return Ok(query.ToList());
    }

    // GET: api/authors/{id}/books
    [HttpGet("{id}/books")]
    public ActionResult<IEnumerable<object>> GetAuthorBooks(int id)
    {
        _logger.LogInformation("Getting books for author with ID: {AuthorId}", id);
        
        var author = Authors.FirstOrDefault(a => a.Id == id);
        if (author == null)
        {
            return NotFound($"Author with ID {id} not found");
        }

        // This would normally be done through Entity Framework relationships
        // For demo purposes, we'll simulate it
        var books = author.Books.Select(b => new
        {
            b.Id,
            b.Title,
            b.ISBN,
            b.Price,
            b.PublishedDate,
            b.StockQuantity
        });

        return Ok(books);
    }

    private static void SeedData()
    {
        Authors.AddRange(new[]
        {
            new Author 
            { 
                Id = 1, 
                FirstName = "Isaac", 
                LastName = "Asimov", 
                Email = "isaac.asimov@example.com", 
                Nationality = "American", 
                DateOfBirth = new DateTime(1920, 1, 2),
                Biography = "Isaac Asimov was an American writer and professor of biochemistry at Boston University. He was known for his works of science fiction and popular science."
            },
            new Author 
            { 
                Id = 2, 
                FirstName = "Agatha", 
                LastName = "Christie", 
                Email = "agatha.christie@example.com", 
                Nationality = "British", 
                DateOfBirth = new DateTime(1890, 9, 15),
                Biography = "Dame Agatha Mary Clarissa Christie was an English writer known for her detective novels, particularly those featuring Hercule Poirot and Miss Jane Marple."
            },
            new Author 
            { 
                Id = 3, 
                FirstName = "Stephen", 
                LastName = "King", 
                Email = "stephen.king@example.com", 
                Nationality = "American", 
                DateOfBirth = new DateTime(1947, 9, 21),
                Biography = "Stephen Edwin King is an American author of horror, supernatural fiction, suspense, crime, science-fiction, and fantasy novels."
            },
            new Author 
            { 
                Id = 4, 
                FirstName = "J.K.", 
                LastName = "Rowling", 
                Email = "jk.rowling@example.com", 
                Nationality = "British", 
                DateOfBirth = new DateTime(1965, 7, 31),
                Biography = "Joanne Rowling, known by her pen name J. K. Rowling, is a British author and philanthropist. She wrote the Harry Potter series."
            },
            new Author 
            { 
                Id = 5, 
                FirstName = "George", 
                LastName = "Orwell", 
                Email = "george.orwell@example.com", 
                Nationality = "British", 
                DateOfBirth = new DateTime(1903, 6, 25),
                Biography = "Eric Arthur Blair, known by his pen name George Orwell, was an English novelist, essayist, journalist and critic."
            }
        });

        _nextAuthorId = 6;
    }
}

public class CreateAuthorRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Nationality { get; set; }
}