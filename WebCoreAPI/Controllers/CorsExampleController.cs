using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models;

namespace WebCoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CorsExampleController : ControllerBase
{
    private readonly ILogger<CorsExampleController> _logger;

    public CorsExampleController(ILogger<CorsExampleController> logger)
    {
        _logger = logger;
    }

    // This endpoint uses the global CORS policy
    [HttpGet("global")]
    public IActionResult GetWithGlobalPolicy()
    {
        _logger.LogInformation("Called endpoint with global CORS policy");
        
        var sampleBooks = new[]
        {
            new { Title = "Foundation", Author = "Isaac Asimov", Price = 15.99m },
            new { Title = "1984", Author = "George Orwell", Price = 13.99m },
            new { Title = "Harry Potter", Author = "J.K. Rowling", Price = 18.99m }
        };
        
        return Ok(new { 
            message = "This endpoint uses the global CORS policy (AllowAll)",
            timestamp = DateTime.UtcNow,
            policy = "AllowAll",
            sampleBooks = sampleBooks
        });
    }

    // This endpoint uses a specific CORS policy
    [HttpGet("specific")]
    [EnableCors("SpecificOrigins")]
    public IActionResult GetWithSpecificPolicy()
    {
        _logger.LogInformation("Called endpoint with specific CORS policy");
        return Ok(new { 
            message = "This endpoint uses SpecificOrigins policy",
            timestamp = DateTime.UtcNow,
            policy = "SpecificOrigins",
            allowedOrigins = new[] { "http://localhost:3000", "https://localhost:3000", "http://127.0.0.1:3000" }
        });
    }

    // This endpoint uses the restrictive CORS policy
    [HttpGet("restrictive")]
    [EnableCors("RestrictivePolicy")]
    public IActionResult GetWithRestrictivePolicy()
    {
        _logger.LogInformation("Called endpoint with restrictive CORS policy");
        return Ok(new { 
            message = "This endpoint uses RestrictivePolicy",
            timestamp = DateTime.UtcNow,
            policy = "RestrictivePolicy",
            allowedMethods = new[] { "GET", "POST", "PUT" },
            allowedHeaders = new[] { "Content-Type", "Authorization" }
        });
    }

    // This endpoint accepts POST requests with CORS
    [HttpPost("book-order")]
    [EnableCors("SpecificOrigins")]
    public IActionResult PostBookOrder([FromBody] BookOrderRequest request)
    {
        _logger.LogInformation("Received book order with CORS");
        
        var order = new
        {
            OrderId = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            BookTitle = request.BookTitle,
            Quantity = request.Quantity,
            TotalPrice = request.Price * request.Quantity,
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };
        
        return Ok(new { 
            message = "Book order received successfully",
            order = order,
            timestamp = DateTime.UtcNow
        });
    }

    // This endpoint disables CORS
    [HttpGet("no-cors")]
    [DisableCors]
    public IActionResult GetWithoutCors()
    {
        _logger.LogInformation("Called endpoint without CORS");
        return Ok(new { 
            message = "This endpoint has CORS disabled",
            timestamp = DateTime.UtcNow,
            note = "This will only work for same-origin requests"
        });
    }

    // Preflight request example
    [HttpOptions("preflight-test")]
    [EnableCors("RestrictivePolicy")]
    public IActionResult PreflightTest()
    {
        return Ok();
    }

    [HttpPut("preflight-test")]
    [EnableCors("RestrictivePolicy")]
    public IActionResult PutWithPreflight([FromBody] object data)
    {
        _logger.LogInformation("PUT request after preflight check");
        return Ok(new { 
            message = "PUT request successful after preflight",
            data = data,
            timestamp = DateTime.UtcNow
        });
    }
}

public class BookOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}