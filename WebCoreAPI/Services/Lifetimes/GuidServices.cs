namespace WebCoreAPI.Services.Lifetimes;

// These three services demonstrate the difference between the DI lifetimes.
// Each one stamps itself with a Guid the moment it is created.
// By comparing the Guids inside a single request (and across requests) you can
// SEE how Singleton / Scoped / Transient behave.
//
//   Singleton -> Same Guid for EVERY request, forever (created once).
//   Scoped    -> Same Guid WITHIN one request, new Guid for the next request.
//   Transient -> A new Guid EVERY time it is injected (even twice in one request).

public interface ISingletonGuidService
{
    Guid OperationId { get; }
}

public interface IScopedGuidService
{
    Guid OperationId { get; }
}

public interface ITransientGuidService
{
    Guid OperationId { get; }
}

public class SingletonGuidService : ISingletonGuidService
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

public class ScopedGuidService : IScopedGuidService
{
    public Guid OperationId { get; } = Guid.NewGuid();
}

public class TransientGuidService : ITransientGuidService
{
    public Guid OperationId { get; } = Guid.NewGuid();
}
