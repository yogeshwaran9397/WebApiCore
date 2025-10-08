using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebCoreAPI.Authorization;
using WebCoreAPI.Services;
using WebCoreAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add custom services
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<UserService>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Global Exception Handling Middleware (should be first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Enable static files
app.UseStaticFiles();

// Use CORS middleware (must be before authentication)
app.UseCors("AllowAll"); // You can change this to use different policies

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

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
