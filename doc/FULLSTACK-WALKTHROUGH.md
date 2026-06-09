# 🧭 Full-Stack Walkthrough — Client + Server

> **Goal of this doc:** after reading it you can sit in an interview and confidently explain, end to end, **how this project is built** and **how every ASP.NET Core Web API concept is implemented** — with a runnable demo to back up every claim.
>
> Pair it with:
> - [`../LEARNING-GUIDE.md`](../LEARNING-GUIDE.md) — the *theory* of each topic (analogies, diagrams, memory aids).
> - [`../client/README.md`](../client/README.md) — how to run the React client.
> - This doc — *how the two halves fit together* + **interview talking points**.

---

## 1. The 30-second pitch

> "It's a **.NET 9 ASP.NET Core Web API** that demonstrates the full Web API curriculum — routing, model binding, validation, DI, caching, logging, filters, JWT auth, role/claims/policy authorization, API versioning, CORS, and global exception handling — plus a **React (Vite) single-page client** that exercises every endpoint. The client logs in, stores the JWT, and auto-attaches it to protected calls, showing the status code, timing, and response for each request."

That sentence alone hits ~12 interview topics. The rest of this doc lets you go deep on any of them.

---

## 2. Architecture at a glance

```
┌───────────────────────────┐         HTTP + JSON (CORS)         ┌────────────────────────────────────────┐
│   React Client (Vite)     │  ───────────────────────────────► │     ASP.NET Core Web API (.NET 9)        │
│   http://localhost:5173   │  ◄─────────────────────────────── │     http://localhost:5274                │
│                           │      Authorization: Bearer JWT     │                                          │
│  apiCatalog.js  (routes)  │                                    │  Middleware pipeline → Routing →          │
│  api.jsx        (call+JWT) │                                    │  Filters → Controller → Service → Data    │
│  Endpoint.jsx   (try-it)  │                                    │                                          │
└───────────────────────────┘                                    └────────────────────────────────────────┘
```

**Two independent processes** that speak only HTTP/JSON. That decoupling *is* the talking point: the front-end doesn't care that the back-end is .NET, and the back-end doesn't care that the client is React — the contract is the HTTP API.

---

## 3. The end-to-end request lifecycle (trace a login)

This is the single most valuable thing to be able to narrate. Follow one click of **Login** through both halves:

```
[CLIENT]
1. User clicks "Send" on the Login card (Endpoint.jsx)
2. call() in api.jsx builds POST http://localhost:5274/api/v1/auth/login
   - sets Content-Type: application/json
   - body: { "username":"admin", "password":"admin123" }
3. Browser fires a CORS preflight (OPTIONS) first → server answers 204 with
   Access-Control-Allow-* headers → browser proceeds with the real POST.

        │  HTTP POST
        ▼
[SERVER]  (order matters — this is the middleware pipeline in Program.cs)
4.  Kestrel receives the request.
5.  UseHttpsRedirection
6.  GlobalExceptionHandlingMiddleware  (wraps everything below in try/catch)
7.  UseStaticFiles
8.  UseCors("AllowAll")                 (adds Access-Control-Allow-Origin)
9.  UseResponseCaching
10. UseAuthentication                   (no token yet — this endpoint is [AllowAnonymous])
11. UseAuthorization
12. Routing matches [Route("api/v{version:apiVersion}/auth")] + [HttpPost("login")]
13. Model binding: [FromBody] LoginRequest  ← JSON deserialized into a C# object
14. [ApiController] auto-validates the model (would 400 if invalid)
15. Filters run (none custom here), then the action executes:
        AuthController.Login()
          → UserService.AuthenticateAsync(user, pass)   (Scoped service via DI)
          → JwtTokenService.GenerateToken(user)         (builds & signs the JWT)
16. Returns Ok(AuthResponse) → serialized to JSON.

        │  HTTP 200 { token, refreshToken, user }
        ▼
[CLIENT]
17. call() parses JSON, returns a normalized result.
18. Endpoint.jsx sees def.capture === 'token' → setToken(body.token)
19. api.jsx saves the token to React state + localStorage.
20. Top bar flips to "🔓 admin". Every 🔒 card now sends Authorization: Bearer <token>.
```

> 🎤 **Interview gold:** being able to say *"authentication and authorization middleware must come after routing but the controller's `[Authorize]` is enforced by the authorization middleware, and `UseCors` must run before them"* shows you understand **pipeline ordering**, the #1 source of real-world bugs.

---

## 4. Server implementation tour

### 4.1 `Program.cs` — two phases

```csharp
var builder = WebApplication.CreateBuilder(args);   // PHASE 1: register services (DI container)
//   AddControllers().AddXmlSerializerFormatters()  → MVC + XML content negotiation
//   AddOpenApi() / AddSwaggerGen()                 → docs
//   AddMemoryCache() / AddResponseCaching()        → caching
//   AddAuthentication().AddJwtBearer(...)          → JWT validation params
//   AddAuthorization(o => o.AddPolicy(...))        → role/claims/policy policies
//   AddApiVersioning(...)                          → 4 version readers combined
//   AddCors(...)                                   → 3 named policies
//   AddScoped/Singleton/Transient<...>            → app services + lifetime demo + filters + validators

var app = builder.Build();                          // PHASE 2: build the middleware pipeline (order matters!)
//   MapOpenApi / UseSwagger / UseSwaggerUI (dev)
//   UseHttpsRedirection
//   UseMiddleware<GlobalExceptionHandlingMiddleware>
//   UseStaticFiles
//   UseCors → UseResponseCaching → UseAuthentication → UseAuthorization
//   MapControllerRoute (conventional) + MapControllers (attribute)
app.Run();
```

> 🧠 **Mantra:** *"Build = register what you need (shopping list). Configure = arrange the pipeline (assembly line)."*

### 4.2 Folder responsibilities

| Folder | Role | Interview hook |
|--------|------|----------------|
| `Controllers/` | HTTP entry points; thin. | "Controllers orchestrate; they don't contain business logic." |
| `Services/` | Business logic, injected via DI. | "`UserService`/`JwtTokenService` are Scoped; `SecurityService` wraps crypto." |
| `Models/` + `Models/Dtos/` | Entities + request/response DTOs. | "Never expose EF entities directly — use DTOs." |
| `Validators/` | FluentValidation rules. | "Validation that's too complex for annotations lives here." |
| `Filters/` | Cross-cutting filter pipeline code. | "Action/Resource/Result/Exception filters." |
| `Authorization/` | Custom requirements + handlers. | "Policy-based auth = requirement + `AuthorizationHandler`." |
| `Middleware/` | Global exception handler. | "Catches everything; returns a consistent error shape." |

### 4.3 The DI lifetime demo (a favorite interview question)

`Services/Lifetimes/GuidServices.cs` registers one service of each lifetime, each stamping a GUID at creation:

```csharp
AddSingleton<ISingletonGuidService,...>(); // one instance for the whole app
AddScoped<IScopedGuidService,...>();        // one instance per HTTP request
AddTransient<ITransientGuidService,...>();  // new instance every injection
```

`LifetimesController` injects **each twice** so you can observe: Singleton identical everywhere, Scoped identical within a request, Transient different even within one request. **Demo:** client section 2, click Send twice.

---

## 5. Client implementation tour

### 5.1 Data-driven design

The whole UI is generated from `src/apiCatalog.js` — an array of topics, each with endpoint descriptors:

```js
{ id:'login', method:'POST', path:'/api/v1/auth/login',
  body:{ username:'admin', password:'admin123' }, capture:'token' }
```

`Endpoint.jsx` turns one descriptor into a card with editable inputs + a Send button. **Adding an endpoint = adding an object** — no new components. This mirrors how you'd build an internal API console.

### 5.2 One HTTP wrapper + auth context (`api.jsx`)

`ApiProvider` is a React **Context** that holds `baseUrl`, the `token` (persisted to `localStorage`), and the `call()` function. `call()` is the client-side analogue of a server `HttpClient` with a delegating handler:

- attaches `Authorization: Bearer <token>` when `auth: true`,
- sets `Content-Type` + serializes JSON bodies,
- normalizes the response to `{ ok, status, durationMs, body, headers }`,
- **never throws on HTTP errors** — a 404 is data to render.

> 🎤 **Interview hook:** "I centralized cross-cutting HTTP concerns (base URL, auth header, JSON, error shaping) in one place, exposed via Context, so components stay declarative." That's the same *separation of concerns* you preach on the server.

---

## 6. Topic-by-topic: implementation + interview Q&A

For each: **where it lives**, **how to demo it**, and a **crisp interview answer**.

### Routing
- **Server:** attribute routing (`[Route("api/[controller]")]`, `[HttpGet("{id:int}")]`) + conventional routes in `Program.cs`. Constraints in `AdvancedRoutingController`.
- **Q: Route constraint gotcha?** "`min`/`max`/`range` are **integer-only**. I hit this with `{price:decimal:min(0.01)}` — it threw at startup, so I constrain `:decimal` and range-check in code."
- **Demo:** client → API Versioning / Bookstore sections.

### Model Binding & Content Negotiation
- **Server:** `ModelBindingController` shows `[FromRoute/Query/Header/Body/Form]`; XML enabled via `AddXmlSerializerFormatters()`.
- **Q: Default body format?** "JSON. `[FromBody]` reads the request body; only one `[FromBody]` per action. Content negotiation picks the response format from the `Accept` header."
- **Demo:** client section 4.

### HTTP Methods
- **Server:** `HttpMethodsController` — full CRUD + HEAD/OPTIONS, plus a standard **JSON Patch (RFC 6902)** endpoint (`json-patch/{id}`) using `JsonPatchDocument<T>` (requires `AddNewtonsoftJson()`).
- **Q: PUT vs PATCH? Idempotent?** "PUT replaces the whole resource (idempotent); PATCH updates sent fields only. GET/PUT/DELETE are idempotent; POST isn't."
- **Q: How do you do a standard partial update?** "JSON Patch — the body is an array of ops (`replace`/`add`/`remove`/`move`/`copy`/`test`) sent as `application/json-patch+json`; `patch.ApplyTo(entity, ModelState)` then re-validate."
- **Demo:** client section 5 (incl. the JSON Patch card).

### Pagination
- **Server:** `PaginationController` shows **offset** (`page/pageSize`) and **cursor/keyset** (`?cursor=...&limit=`) with an opaque Base64 `nextCursor`.
- **Q: Offset vs cursor — when and why?** "Offset is simple and supports 'jump to page N' but is slow on deep pages and can skip/duplicate rows if data changes. Cursor/keyset (`WHERE Id > lastSeen ORDER BY Id LIMIT n`) is index-friendly, constant-speed, and stable — ideal for infinite scroll and large datasets."
- **Demo:** client section 5b — call cursor with no cursor, then paste `nextCursor` for the next page.

### Status Codes
- **Server:** `StatusCodesController` returns each code via `Ok/Created/NoContent/BadRequest/StatusCode`.
- **Q: 401 vs 403?** "401 = not authenticated (who are you?); 403 = authenticated but not allowed."
- **Demo:** client section 3.

### Validation
- **Server:** Data Annotations on `ProductDto` (auto-400 via `[ApiController]`) **and** FluentValidation (`ProductDtoValidator` on annotation-free `ProductInput`).
- **Q: Why a separate input model for Fluent?** "`[ApiController]` validates annotations first and short-circuits with a 400, so the Fluent endpoint uses an annotation-free model to let Fluent rules run — shows I understand the validation order."
- **Demo:** client section 6 (invalid bodies pre-loaded).

### Dependency Injection
- **Server:** constructor injection everywhere; lifetime demo (§4.3).
- **Q: Which lifetime for DbContext?** "Scoped — one per request, matches the unit-of-work boundary. Never inject Scoped into a Singleton (captive dependency)."
- **Demo:** client section 2 (Send twice).

### Caching
- **Server:** `IMemoryCache` + `[ResponseCache]` in `CachingController`; `UseResponseCaching` middleware.
- **Q: In-memory vs distributed?** "In-memory is per-server RAM, lost on restart, breaks under load-balancing; distributed (Redis) is shared across instances. Response caching is HTTP-level (Cache-Control)."
- **Demo:** client section 7 — call `memory` twice in 30s, `generatedAt` stays frozen.

### Logging
- **Server:** `ILogger<T>` with structured templates, exceptions, and scopes in `LoggingDemoController`.
- **Q: Why structured logging?** "`{Placeholder}` values are captured as searchable properties by sinks like Serilog/Seq/ELK, not just baked into a string."
- **Demo:** client section 8, then watch the dotnet console.

### Filters
- **Server:** `Filters/DemoFilters.cs` (Action/Resource/Result/Exception) applied via `[ServiceFilter]`/`[TypeFilter]`.
- **Q: Filter vs middleware?** "Filters run inside MVC (have access to model binding, action context, `ModelState`); middleware is lower-level and runs for every request. Use filters for MVC-specific cross-cutting, middleware for app-wide."
- **Q: ServiceFilter vs TypeFilter?** "Both support DI; only TypeFilter takes constructor arguments."
- **Demo:** client section 9 — watch console order + the `X-Demo-Result-Filter` header in the Headers tab.

### Security (crypto)
- **Server:** `SecurityService` — PBKDF2 password hashing (salt + constant-time compare), AES encryption, HMAC.
- **Q: How store passwords?** "Never plaintext. Salted one-way hash (PBKDF2/BCrypt/Argon2), compare with a constant-time function to avoid timing attacks."
- **Demo:** client section 10.

### JWT Authentication
- **Server:** `AddJwtBearer` with `TokenValidationParameters` (issuer/audience/lifetime/signing key); `JwtTokenService` issues tokens; `AuthController` login/register/me.
- **Q: JWT structure? Is the payload secret?** "header.payload.signature, base64url. Payload is **not encrypted** — only signed — so never put secrets in it. The signature (HMAC with the server secret) makes it tamper-proof."
- **Q: Stateless logout?** "JWTs can't be 'deleted' server-side; you use short expiry + refresh tokens stored in a DB so they can be revoked."
- **Demo:** client section 1 → token captured → top bar shows 🔓.

### Authorization (role / claims / policy)
- **Server:** policies in `Program.cs`; custom requirements + handlers in `Authorization/`. Controllers `role-demo`, `claims-demo`, `policy-demo`.
- **Q: Role vs claim vs policy?** "Roles are a coarse claim. Claims-based checks any claim. Policy-based is the most flexible — a named policy backed by requirements + an `AuthorizationHandler` for complex logic."
- **Demo:** client section 11 — log in as `admin` (200) vs `user` (403).

### API Versioning
- **Server:** `AddApiVersioning` with `ApiVersionReader.Combine(UrlSegment, QueryString, Header, MediaType)`; `V1`/`V2` controllers.
- **Q: Best strategy?** "URL path (`/api/v2/...`) is the most popular — explicit and cacheable. This project supports all four simultaneously."
- **Demo:** client section 12 — `/api/v1/books` vs `/api/v2/books`, or `?version=2.0`.

### CORS
- **Server:** three named policies; `UseCors("AllowAll")` (dev). Preflight handled automatically.
- **Q: Why does CORS exist / why preflight?** "Browser same-origin policy blocks cross-origin calls unless the server opts in via `Access-Control-Allow-*`. For non-simple requests the browser sends an OPTIONS preflight first."
- **Demo:** the whole client *is* the CORS demo — it's a cross-origin SPA hitting the API.

### Rate Limiting (sliding window)
- **Server:** `AddRateLimiter` + `AddSlidingWindowLimiter("sliding", …)` in `Program.cs` (5 req / 15s, 3 segments); `app.UseRateLimiter()`; `RateLimitController` opts in with `[EnableRateLimiting("sliding")]`, and one action opts out with `[DisableRateLimiting]`.
- **Q: Fixed vs sliding window?** "A fixed window resets abruptly, so you can get a 2× burst across the boundary (5 at 0:59 + 5 at 1:00). A sliding window divides the window into segments and expires the oldest continuously, so the rate holds smoothly. .NET also has token-bucket and concurrency limiters."
- **Q: What does the server send when throttled?** "HTTP 429 Too Many Requests with a `Retry-After` header telling the client how long to back off."
- **Demo:** client section 12b → **Burst ×8** → 5 succeed, 3 return 429.

### Global Exception Handling
- **Server:** `GlobalExceptionHandlingMiddleware` registered early; returns a consistent JSON error with a correlation id, stack trace only in Development.
- **Q: Middleware vs exception filter for errors?** "Middleware catches everything in the pipeline (including non-MVC); an exception filter only catches MVC action errors. I use middleware for app-wide handling."

---

## 7. Likely interview questions — rapid-fire answers

| Question | One-liner answer |
|----------|------------------|
| What is middleware? | Components in a pipeline; each can act on the request, call `next`, then act on the response. Order matters. |
| `IActionResult` vs `ActionResult<T>`? | `ActionResult<T>` gives you typed returns **and** status-code helpers; better for OpenAPI. |
| Where does model validation happen? | Automatically with `[ApiController]` before the action; returns 400 `ProblemDetails`. |
| Kestrel? | The built-in cross-platform web server; usually behind a reverse proxy (IIS/Nginx) in prod. |
| In-process vs out-of-process hosting? | In-process runs inside the IIS worker (faster); out-of-process runs Kestrel in a separate process with IIS as reverse proxy. |
| How is DI configured? | `builder.Services.Add{Singleton,Scoped,Transient}` in `Program.cs`; constructor injection. |
| Captive dependency? | Injecting a shorter-lived service (Scoped) into a longer-lived one (Singleton) — it gets "stuck" and never updates. |
| How do you secure an API? | HTTPS + JWT bearer auth + authorization policies + hashed passwords + input validation + CORS. |
| Async all the way? | Controllers/services return `Task<...>` so threads aren't blocked on I/O — scales better. |
| How does the client keep the user logged in? | JWT in `localStorage`, re-attached on each request; React Context exposes it app-wide. |

---

## 8. How to give a live demo (3 minutes)

1. `dotnet run --project WebCoreAPI` and `npm run dev` (in `client/`).
2. Open **Swagger** (`/swagger`) — "here's the auto-generated contract."
3. In the React client: **Login** → point at the top bar flipping to 🔓 and explain token capture.
4. **DI Lifetimes** → Send twice → explain Singleton/Scoped/Transient from the GUIDs.
5. **Authorization** → call `admin-only` as admin (200), logout, login as `user`, call again (403).
6. **Validation** → send the invalid body → show the auto-400 vs Fluent errors.
7. **Filters** → Send → show the `X-Demo-Result-Filter` header + console order.

That sequence demonstrates: DI, auth, authorization, validation, filters, status codes, and the full request lifecycle — most of the curriculum in three minutes.
