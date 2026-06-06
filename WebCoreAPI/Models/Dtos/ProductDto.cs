using System.ComponentModel.DataAnnotations;

namespace WebCoreAPI.Models.Dtos;

/// <summary>
/// A DTO that demonstrates validation using Data Annotations.
/// When [ApiController] is applied to a controller, ASP.NET Core automatically
/// validates this object and returns a 400 with the errors if any rule fails.
/// </summary>
public class ProductDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000.")]
    public decimal Price { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "ContactEmail must be a valid email address.")]
    public string ContactEmail { get; set; } = string.Empty;

    [RegularExpression(@"^\d{5}$", ErrorMessage = "ZipCode must be exactly 5 digits.")]
    public string ZipCode { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    // Used by the FluentValidation example (conditional rule).
    public bool IsPhysical { get; set; }

    public double Weight { get; set; }
}

/// <summary>
/// Same shape as <see cref="ProductDto"/> but WITHOUT data annotations.
/// Used by the FluentValidation endpoint so that [ApiController]'s automatic
/// annotation validation does not short-circuit before FluentValidation runs.
/// </summary>
public class ProductInput
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public int Stock { get; set; }
    public bool IsPhysical { get; set; }
    public double Weight { get; set; }
}
