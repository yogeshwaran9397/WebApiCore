using FluentValidation;
using WebCoreAPI.Models.Dtos;

namespace WebCoreAPI.Validators;

/// <summary>
/// FluentValidation alternative to Data Annotations.
/// More powerful: supports conditional, custom, and async rules in a fluent style.
/// Registered in Program.cs and resolved manually in ValidationController.
/// </summary>
public class ProductDtoValidator : AbstractValidator<ProductInput>
{
    public ProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Length(3, 100).WithMessage("Name must be 3-100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(100000);

        RuleFor(x => x.ContactEmail)
            .NotEmpty()
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.ZipCode)
            .Matches(@"^\d{5}$").WithMessage("ZipCode must be exactly 5 digits.");

        // Conditional rule: weight only matters for physical products.
        When(x => x.IsPhysical, () =>
        {
            RuleFor(x => x.Weight)
                .GreaterThan(0)
                .WithMessage("Physical products must have a weight greater than 0.");
        });

        // Custom rule example.
        RuleFor(x => x.Name)
            .Must(name => !name.Contains("test", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Name cannot contain the word 'test'.");
    }
}
