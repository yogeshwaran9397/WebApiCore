using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates two pagination strategies:
///
///   OFFSET (page/pageSize)  — simple, but slow on deep pages and can SKIP or
///                             DUPLICATE rows if data changes between requests.
///   CURSOR (keyset)         — pass an opaque pointer to the LAST item you saw;
///                             the server returns the next slice WHERE Id > cursor.
///                             Stable under inserts/deletes and fast at any depth.
/// </summary>
[ApiController]
[Route("api/pagination")]
public class PaginationController : ControllerBase
{
    // A stable, ordered demo dataset (sorted by Id).
    private static readonly List<Item> Data = Enumerable.Range(1, 53)
        .Select(i => new Item { Id = i, Name = $"Item #{i:D2}", Value = i * 10 })
        .ToList();

    /// <summary>
    /// OFFSET pagination — the classic approach. Good for "jump to page N" UIs.
    /// Downside: WHERE ... OFFSET 100000 still scans/skips all preceding rows.
    /// </summary>
    [HttpGet("offset")]
    public IActionResult Offset([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 10;

        var items = Data.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            strategy = "offset",
            page,
            pageSize,
            totalItems = Data.Count,
            totalPages = (int)Math.Ceiling((double)Data.Count / pageSize),
            items
        });
    }

    /// <summary>
    /// CURSOR (keyset) pagination.
    /// First call: omit "cursor". Each response returns a "nextCursor" — pass it back
    /// to fetch the next slice. The cursor is an opaque, Base64-encoded pointer to the
    /// last Id returned, so clients treat it as a black box.
    /// </summary>
    [HttpGet("cursor")]
    public IActionResult Cursor([FromQuery] string? cursor = null, [FromQuery] int limit = 10)
    {
        if (limit is < 1 or > 100) limit = 10;

        // Decode the cursor → the last Id the client has already seen (0 = start).
        var afterId = DecodeCursor(cursor);

        // KEYSET query: take the next `limit` items strictly AFTER that Id.
        // (In EF Core this becomes: WHERE Id > afterId ORDER BY Id LIMIT n — index-friendly.)
        var slice = Data.Where(x => x.Id > afterId)
                        .OrderBy(x => x.Id)
                        .Take(limit + 1)        // fetch one extra to detect "hasMore"
                        .ToList();

        var hasMore = slice.Count > limit;
        var items = slice.Take(limit).ToList();
        var nextCursor = hasMore && items.Count > 0 ? EncodeCursor(items[^1].Id) : null;

        return Ok(new
        {
            strategy = "cursor (keyset)",
            limit,
            count = items.Count,
            hasMore,
            nextCursor,                 // pass this back as ?cursor=... for the next page
            items
        });
    }

    // ---- Opaque cursor helpers (Base64 of "id:{lastId}") ----

    private static string EncodeCursor(int lastId)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"id:{lastId}"));

    private static int DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor)) return 0;
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            return raw.StartsWith("id:") && int.TryParse(raw[3..], out var id) ? id : 0;
        }
        catch
        {
            return 0; // malformed cursor → start from the beginning
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
