using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models.Dtos;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates the 5 model-binding sources plus content negotiation.
///
///   [FromRoute]  -> value from the URL path        /api/binding/route/5
///   [FromQuery]  -> value from the query string     /api/binding/query?page=2&size=10
///   [FromHeader] -> value from an HTTP header        X-Client-Id: abc
///   [FromBody]   -> value from the JSON/XML body     { ... }
///   [FromForm]   -> value from multipart form data   (file uploads)
/// </summary>
[ApiController]
[Route("api/binding")]
public class ModelBindingController : ControllerBase
{
    // [FromRoute] - bind from the URL path segment
    [HttpGet("route/{id}")]
    public IActionResult FromRouteExample([FromRoute] int id)
        => Ok(new { source = "FromRoute", id });

    // [FromQuery] - bind from ?key=value pairs (great for filters & pagination)
    [HttpGet("query")]
    public IActionResult FromQueryExample([FromQuery] int page = 1, [FromQuery] int size = 10)
        => Ok(new { source = "FromQuery", page, size });

    // [FromHeader] - bind from an HTTP header (API keys, versions, correlation ids)
    [HttpGet("header")]
    public IActionResult FromHeaderExample([FromHeader(Name = "X-Client-Id")] string? clientId)
        => Ok(new { source = "FromHeader", clientId = clientId ?? "(header not sent)" });

    // [FromBody] - bind a complex object from the request body (JSON by default)
    [HttpPost("body")]
    public IActionResult FromBodyExample([FromBody] ProductDto product)
        => Ok(new { source = "FromBody", product });

    // [FromForm] - bind from multipart/form-data, including file uploads
    [HttpPost("form")]
    public IActionResult FromFormExample([FromForm] string name, IFormFile? file)
        => Ok(new
        {
            source = "FromForm",
            name,
            fileName = file?.FileName ?? "(no file)",
            sizeBytes = file?.Length ?? 0
        });

    // Combining sources in one action: id from route, fields from body.
    [HttpPut("combined/{id}")]
    public IActionResult Combined([FromRoute] int id, [FromBody] ProductDto product)
        => Ok(new { source = "FromRoute + FromBody", id, product });

    // Content negotiation + Produces/Consumes:
    // - Consumes: this action only accepts application/json bodies.
    // - Produces: it advertises that it returns application/json.
    // Try sending "Accept: application/xml" (XML formatters must be enabled to honor it).
    [HttpPost("negotiation")]
    [Consumes("application/json")]
    [Produces("application/json", "application/xml")]
    public IActionResult ContentNegotiation([FromBody] ProductDto product)
        => Ok(new { source = "Content Negotiation", accept = Request.Headers.Accept.ToString(), product });
}
