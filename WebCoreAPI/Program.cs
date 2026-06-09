using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using WebCoreAPI.Authorization;
using WebCoreAPI.Filters;
using WebCoreAPI.Models.Dtos;
using WebCoreAPI.Services;
using WebCoreAPI.Services.Lifetimes;
using WebCoreAPI.Validators;
using WebCoreAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// AddXmlSerializerFormatters enables CONTENT NEGOTIATION to XML (Accept: application/xml).
// AddNewtonsoftJson registers the input formatter required for JsonPatchDocument
// (RFC 6902, media type application/json-patch+json). We force camelCase so the
// JSON output stays identical to the System.Text.Json default used elsewhere.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver =
            new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    })
    .AddXmlSerializerFormatters();

builder.Services.AddOpenApi();

// Swagger UI (Swashbuckle) - interactive API docs at /swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Caching services
builder.Services.AddMemoryCache();        // In-memory cache (IMemoryCache)
builder.Services.AddResponseCaching();    // HTTP response caching middleware

// Add custom services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SecurityService>();

// DI LIFETIME DEMO services - one of each lifetime, registered to the matching method.
builder.Services.AddSingleton<ISingletonGuidService, SingletonGuidService>();
builder.Services.AddScoped<IScopedGuidService, ScopedGuidService>();
builder.Services.AddTransient<ITransientGuidService, TransientGuidService>();

// FluentValidation - register validators so they can be injected as IValidator<T>.
builder.Services.AddScoped<IValidator<ProductInput>, ProductDtoValidator>();

// Filters - registered in DI so [ServiceFilter]/[TypeFilter] can resolve them.
builder.Services.AddScoped<LoggingActionFilter>();
builder.Services.AddScoped<TimingResourceFilter>();
builder.Services.AddScoped<CustomResultFilter>();
builder.Services.AddScoped<DemoExceptionFilter>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "WebCoreAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "WebCoreAPIUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT Token validated for user: {Username}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("UserOrAbove", policy => policy.RequireRole("Admin", "Manager", "User"));
    options.AddPolicy("SupportAccess", policy => policy.RequireRole("Admin", "Manager", "Support"));

    // Claims-based policies
    options.AddPolicy("HighSecurityLevel", policy => 
        policy.RequireClaim("security_level", "3", "4", "5"));
    options.AddPolicy("ITDepartment", policy => 
        policy.RequireClaim("department", "IT"));
    options.AddPolicy("ExecutiveAccess", policy => 
        policy.RequireClaim("salary_band", "Executive", "Manager"));

    // Custom policy-based authorization
    options.AddPolicy("SecurityLevel2", policy => 
        policy.Requirements.Add(new SecurityLevelRequirement(2)));
    options.AddPolicy("SecurityLevel3", policy => 
        policy.Requirements.Add(new SecurityLevelRequirement(3)));
    options.AddPolicy("SecurityLevel4", policy => 
        policy.Requirements.Add(new SecurityLevelRequirement(4)));
    options.AddPolicy("SecurityLevel5", policy => 
        policy.Requirements.Add(new SecurityLevelRequirement(5)));

    options.AddPolicy("ITOrSalesDepartment", policy => 
        policy.Requirements.Add(new DepartmentRequirement("IT", "Sales")));
    options.AddPolicy("CustomerServiceDepartment", policy => 
        policy.Requirements.Add(new DepartmentRequirement("Customer Service")));

    options.AddPolicy("CanReadUsers", policy => 
        policy.Requirements.Add(new PermissionRequirement("users.read")));
    options.AddPolicy("CanWriteUsers", policy => 
        policy.Requirements.Add(new PermissionRequirement("users.write")));
    options.AddPolicy("CanDeleteUsers", policy => 
        policy.Requirements.Add(new PermissionRequirement("users.delete")));
    options.AddPolicy("CanManageBooks", policy => 
        policy.Requirements.Add(new PermissionRequirement("books.write")));

    options.AddPolicy("NorthAmericaRegion", policy => 
        policy.Requirements.Add(new RegionRequirement("North America", "Global")));
    options.AddPolicy("EuropeRegion", policy => 
        policy.Requirements.Add(new RegionRequirement("Europe", "Global")));

    // Composite policies
    options.AddPolicy("HighLevelManager", policy => 
        policy.Requirements.Add(new CompositeRequirement(
            minimumSecurityLevel: 3,
            requiredRoles: new[] { "Admin", "Manager" },
            requiredPermissions: new[] { "users.read", "reports.read" })));
    
    options.AddPolicy("SystemAdministrator", policy => 
        policy.Requirements.Add(new CompositeRequirement(
            minimumSecurityLevel: 4,
            requiredRoles: new[] { "Admin" },
            requiredPermissions: new[] { "system.admin", "users.write", "users.delete" })));

    // Combination policies using multiple requirements
    options.AddPolicy("SeniorStaff", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new SecurityLevelRequirement(2));
        policy.Requirements.Add(new DepartmentRequirement("IT", "Sales", "Customer Service"));
    });
});

// Register authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, SecurityLevelHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DepartmentHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RegionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CompositeHandler>();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    // Default version when none is specified
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Configure how versions are read from requests
    options.ApiVersionReader = ApiVersionReader.Combine(
        // URL path versioning: /api/v1/books, /api/v2/books
        new UrlSegmentApiVersionReader(),
        // Query string versioning: /api/books?version=1.0
        new QueryStringApiVersionReader("version"),
        // Header versioning: X-Version: 1.0
        new HeaderApiVersionReader("X-Version"),
        // Media type versioning: Accept: application/json;version=1.0
        new MediaTypeApiVersionReader("version")
    );

    // Configure version format
    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
}).AddMvc().AddApiExplorer(setup =>
{
    // Format version as 'v{version}'
    setup.GroupNameFormat = "'v'VVV";

    // Automatically substitute version in controller names
    setup.SubstituteApiVersionInUrl = true;
});


// Add CORS services
builder.Services.AddCors(options =>
{
    // Policy 1: Allow all origins (for development only)
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

    // Policy 2: Specific origins (recommended for production)
    options.AddPolicy("SpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://127.0.0.1:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });

    // Policy 3: Specific methods and headers
    options.AddPolicy("RestrictivePolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://myapp.com")
                  .WithMethods("GET", "POST", "PUT")
                  .WithHeaders("Content-Type", "Authorization")
                  .AllowCredentials();
        });
});

// ---------------------------------------------------------------------------
// RATE LIMITING — SLIDING WINDOW
// A fixed window (e.g. "5 per minute") resets abruptly, allowing a burst right
// at the boundary (5 at 0:59 + 5 at 1:00 = 10 in 2 seconds). A SLIDING window
// splits the window into segments and continuously expires the oldest segment,
// so the limit holds smoothly across the boundary.
//
//   PermitLimit=5, Window=15s, SegmentsPerWindow=3  →  each segment = 5s.
//   Every 5s the oldest segment's permits are recycled back into the pool.
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 5;                                  // max requests per window
        opt.Window = TimeSpan.FromSeconds(15);               // total window length
        opt.SegmentsPerWindow = 3;                           // window split into 3 x 5s slices
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                                  // reject immediately, don't queue
    });

    // What to send when a request is throttled.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        // Tell the client how long to wait. Use the limiter's metadata when present,
        // otherwise fall back to one segment length (Window / SegmentsPerWindow = 5s),
        // which is the worst-case time until a sliding-window permit frees up.
        var retrySeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? (int)retryAfter.TotalSeconds
            : 5;
        context.HttpContext.Response.Headers.RetryAfter = retrySeconds.ToString();

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\":\"Too Many Requests\",\"message\":\"Sliding-window rate limit exceeded. Please slow down.\"}",
            token);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger interactive docs at /swagger (development only, for security).
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebCoreAPI v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Global Exception Handling Middleware (should be first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Enable static files
app.UseStaticFiles();

// Use CORS middleware (must be before authentication)
app.UseCors("AllowAll"); // You can change this to use different policies

// Response caching middleware (must be before endpoints, after CORS).
app.UseResponseCaching();

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting middleware (after routing/auth, before endpoints).
// Endpoints opt in with [EnableRateLimiting("sliding")].
app.UseRateLimiter();

// Configure conventional routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Configure API conventional routing
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Get}/{id?}");

// Configure custom conventional routes for bookstore
app.MapControllerRoute(
    name: "bookstore",
    pattern: "bookstore/{controller}/{action}/{id?}",
    defaults: new { controller = "Books", action = "Get" });

// Configure category-specific conventional route
app.MapControllerRoute(
    name: "category_books",
    pattern: "categories/{categoryId:int}/books/{action=GetByCategory}",
    defaults: new { controller = "ConventionalBooks" });

// Configure author-specific conventional route  
app.MapControllerRoute(
    name: "author_books",
    pattern: "authors/{authorId:int}/books/{action=GetByAuthor}",
    defaults: new { controller = "ConventionalBooks" });

// Map attribute-routed controllers (existing controllers)
app.MapControllers();

app.Run();
