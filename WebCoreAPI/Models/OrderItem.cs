namespace WebCoreAPI.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    // Foreign Keys
    public int OrderId { get; set; }
    public int BookId { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Book Book { get; set; } = null!;
}