using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Services.Lifetimes;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Demonstrates Singleton vs Scoped vs Transient service lifetimes.
///
/// Call GET /api/lifetimes TWICE and compare the Guids:
///   - Singleton  : same value in BOTH responses (created once for the whole app).
///   - Scoped     : two values inside ONE response are equal, but differ between requests.
///   - Transient  : the two values inside ONE response are DIFFERENT (new each injection).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LifetimesController : ControllerBase
{
    // Each lifetime is injected TWICE so we can compare instances within a single request.
    private readonly ISingletonGuidService _singleton1;
    private readonly ISingletonGuidService _singleton2;
    private readonly IScopedGuidService _scoped1;
    private readonly IScopedGuidService _scoped2;
    private readonly ITransientGuidService _transient1;
    private readonly ITransientGuidService _transient2;

    public LifetimesController(
        ISingletonGuidService singleton1, ISingletonGuidService singleton2,
        IScopedGuidService scoped1, IScopedGuidService scoped2,
        ITransientGuidService transient1, ITransientGuidService transient2)
    {
        _singleton1 = singleton1;
        _singleton2 = singleton2;
        _scoped1 = scoped1;
        _scoped2 = scoped2;
        _transient1 = transient1;
        _transient2 = transient2;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Note = "Call this endpoint twice and compare. Read the explanation below.",
            Singleton = new
            {
                Instance1 = _singleton1.OperationId,
                Instance2 = _singleton2.OperationId,
                SameWithinRequest = _singleton1.OperationId == _singleton2.OperationId,
                Expectation = "ALWAYS the same value, even across different requests."
            },
            Scoped = new
            {
                Instance1 = _scoped1.OperationId,
                Instance2 = _scoped2.OperationId,
                SameWithinRequest = _scoped1.OperationId == _scoped2.OperationId,
                Expectation = "Same within ONE request, but changes on the NEXT request."
            },
            Transient = new
            {
                Instance1 = _transient1.OperationId,
                Instance2 = _transient2.OperationId,
                SameWithinRequest = _transient1.OperationId == _transient2.OperationId,
                Expectation = "DIFFERENT every time it is injected, even in the same request."
            }
        });
    }
}
