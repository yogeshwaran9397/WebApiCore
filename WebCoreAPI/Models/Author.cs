namespace WebCoreAPI.Models;

public class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public string FullName => $"{FirstName} {LastName}";
}