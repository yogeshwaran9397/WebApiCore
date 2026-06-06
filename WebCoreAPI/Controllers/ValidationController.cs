using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models.Dtos;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates two validation approaches:
///   1. Data Annotations  -> automatic 400 thanks to [ApiController].
///   2. FluentValidation  -> explicit, supports conditional/custom/async rules.
/// </summary>
[ApiController]
[Route("api/validation")]
public class ValidationController : ControllerBase
{
    private readonly IValidator<ProductInput> _fluentValidator;

    public ValidationController(IValidator<ProductInput> fluentValidator)
    {
        _fluentValidator = fluentValidator;
    }

    /// <summary>
    /// Data Annotations: [ApiController] auto-validates the model BEFORE this code runs.
    /// Send an invalid body and you'll get a 400 with a ProblemDetails error list
    /// without any code here. ModelState is guaranteed valid if execution reaches here.
    /// </summary>
    [HttpPost("data-annotations")]
    public IActionResult ValidateWithAnnotations([FromBody] ProductDto product)
    {
        return Ok(new { message = "Passed Data Annotation validation.", product });
    }

    /// <summary>
    /// FluentValidation: we run the validator ourselves and shape the response.
    /// </summary>
    [HttpPost("fluent")]
    public IActionResult ValidateWithFluent([FromBody] ProductInput product)
    {
        var result = _fluentValidator.Validate(product);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { message = "FluentValidation failed.", errors });
        }

        return Ok(new { message = "Passed FluentValidation.", product });
    }
}
