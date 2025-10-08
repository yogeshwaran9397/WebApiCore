using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowAll")]
public class CategoriesController : ControllerBase
{
    private readonly ILogger<CategoriesController> _logger;
    
    // In-memory storage for demo purposes
    private static readonly List<Category> Categories = new();
    private static int _nextCategoryId = 1;

    static CategoriesController()
    {
        SeedData();
    }

    public CategoriesController(ILogger<CategoriesController> logger)
    {
        _logger = logger;
    }

    // GET: api/categories
    [HttpGet]
    public ActionResult<IEnumerable<Category>> GetCategories()
    {
        _logger.LogInformation("Getting all categories");
        return Ok(Categories.Where(c => c.IsActive));
    }

    // GET: api/categories/{id}
    [HttpGet("{id}")]
    public ActionResult<Category> GetCategory(int id)
    {
        _logger.LogInformation("Getting category with ID: {CategoryId}", id);
        
        var category = Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found");
        }

        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public ActionResult<Category> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        _logger.LogInformation("Creating new category: {Name}", request.Name);

        var category = new Category
        {
            Id = _nextCategoryId++,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        Categories.Add(category);
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    // PUT: api/categories/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId}", id);
        
        var category = Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        return NoContent();
    }

    // DELETE: api/categories/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(int id)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId}", id);
        
        var category = Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found");
        }

        Categories.Remove(category);
        return NoContent();
    }

    // GET: api/categories/{id}/books
    [HttpGet("{id}/books")]
    public ActionResult<IEnumerable<object>> GetCategoryBooks(int id)
    {
        _logger.LogInformation("Getting books for category with ID: {CategoryId}", id);
        
        var category = Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found");
        }

        // This would normally be done through Entity Framework relationships
        // For demo purposes, we'll simulate it
        var books = category.Books.Select(b => new
        {
            b.Id,
            b.Title,
            b.ISBN,
            b.Price,
            AuthorName = b.Author.FullName,
            b.StockQuantity
        });

        return Ok(books);
    }

    // GET: api/categories/stats
    [HttpGet("stats")]
    public ActionResult<object> GetCategoryStats()
    {
        _logger.LogInformation("Getting category statistics");
        
        var stats = Categories.Select(c => new
        {
            c.Id,
            c.Name,
            BookCount = c.Books.Count,
            TotalValue = c.Books.Sum(b => b.Price * b.StockQuantity),
            c.IsActive
        });

        return Ok(stats);
    }

    private static void SeedData()
    {
        Categories.AddRange(new[]
        {
            new Category 
            { 
                Id = 1, 
                Name = "Fiction", 
                Description = "Fictional literature including novels, short stories, and novellas",
                IsActive = true
            },
            new Category 
            { 
                Id = 2, 
                Name = "Non-Fiction", 
                Description = "Factual books including biographies, history, science, and self-help",
                IsActive = true
            },
            new Category 
            { 
                Id = 3, 
                Name = "Science Fiction", 
                Description = "Speculative fiction dealing with futuristic concepts and advanced science",
                IsActive = true
            },
            new Category 
            { 
                Id = 4, 
                Name = "Mystery", 
                Description = "Detective stories, crime novels, and suspenseful thrillers",
                IsActive = true
            },
            new Category 
            { 
                Id = 5, 
                Name = "Biography", 
                Description = "Life stories, memoirs, and autobiographies",
                IsActive = true
            },
            new Category 
            { 
                Id = 6, 
                Name = "Fantasy", 
                Description = "Magical and supernatural fiction with fantastical elements",
                IsActive = true
            },
            new Category 
            { 
                Id = 7, 
                Name = "Horror", 
                Description = "Frightening and suspenseful literature designed to create tension",
                IsActive = true
            },
            new Category 
            { 
                Id = 8, 
                Name = "Romance", 
                Description = "Love stories and romantic relationships",
                IsActive = false // Example of inactive category
            }
        });

        _nextCategoryId = 9;
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}