using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models.Dtos;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates every HTTP method mapped to CRUD.
///   GET = Read | POST = Create | PUT = Replace | PATCH = Partial update
///   DELETE = Remove | HEAD = headers only | OPTIONS = supported methods
/// </summary>
[ApiController]
[Route("api/http-methods")]
public class HttpMethodsController : ControllerBase
{
    // Simple in-memory store so the verbs do something visible.
    private static readonly Dictionary<int, ProductDto> Store = new()
    {
        [1] = new ProductDto { Name = "Sample Widget", Price = 9.99m, ContactEmail = "shop@example.com", ZipCode = "12345", Stock = 5 }
    };
    private static int _nextId = 2;

    // GET (read) - safe & idempotent, no body
    [HttpGet]
    public IActionResult GetAll() => Ok(Store.Select(kv => new { id = kv.Key, product = kv.Value }));

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Store.TryGetValue(id, out var p) ? Ok(new { id, product = p }) : NotFound();

    // POST (create) - NOT idempotent: each call creates a new resource
    [HttpPost]
    public IActionResult Create([FromBody] ProductDto product)
    {
        var id = _nextId++;
        Store[id] = product;
        return CreatedAtAction(nameof(GetById), new { id }, new { id, product });
    }

    // PUT (replace) - idempotent: replaces the WHOLE resource
    [HttpPut("{id}")]
    public IActionResult Replace(int id, [FromBody] ProductDto product)
    {
        Store[id] = product; // full replacement
        return Ok(new { method = "PUT (full replace)", id, product });
    }

    // PATCH (partial update) - only the supplied fields change
    [HttpPatch("{id}")]
    public IActionResult Patch(int id, [FromBody] ProductPatch patch)
    {
        if (!Store.TryGetValue(id, out var existing)) return NotFound();
        if (patch.Name is not null) existing.Name = patch.Name;
        if (patch.Price is not null) existing.Price = patch.Price.Value;
        if (patch.Stock is not null) existing.Stock = patch.Stock.Value;
        return Ok(new { method = "PATCH (partial update)", id, product = existing });
    }

    // DELETE (remove) - idempotent: deleting twice is fine
    [HttpDelete("{id}")]
    public IActionResult Remove(int id)
    {
        Store.Remove(id);
        return NoContent(); // 204
    }

    // HEAD - like GET but returns headers only, no body (used to check existence/size)
    [HttpHead("{id}")]
    public IActionResult Head(int id)
        => Store.ContainsKey(id) ? Ok() : NotFound();

    // OPTIONS - advertises which methods this endpoint supports (CORS preflight uses this)
    [HttpOptions]
    public IActionResult Options()
    {
        Response.Headers.Allow = "GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS";
        return Ok(new { supportedMethods = "GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS" });
    }

    // Body shape used only for PATCH (nullable = "only update what is sent").
    public class ProductPatch
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
    }
}
