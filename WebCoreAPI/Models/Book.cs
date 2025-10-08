namespace WebCoreAPI.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime PublishedDate { get; set; }
    public int Pages { get; set; }
    public string Language { get; set; } = "English";
    public string? Publisher { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }

    // Navigation properties
    public virtual Author Author { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}