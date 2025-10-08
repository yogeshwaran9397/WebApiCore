# ASP.NET Core Web API Routing Guide

A comprehensive demonstration of both **Conventional Routing** and **Attribute Routing** in ASP.NET Core Web API.

## 🛣️ **Routing Overview**

ASP.NET Core supports two main routing approaches:
1. **Conventional Routing**: Routes defined centrally in `Program.cs`
2. **Attribute Routing**: Routes defined using attributes on controllers/actions

## 📋 **Route Configuration (Program.cs)**

```csharp
// Default conventional route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API conventional route  
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Get}/{id?}");

// Custom bookstore route
app.MapControllerRoute(
    name: "bookstore", 
    pattern: "bookstore/{controller}/{action}/{id?}",
    defaults: new { controller = "Books", action = "Get" });

// Category-specific route
app.MapControllerRoute(
    name: "category_books",
    pattern: "categories/{categoryId:int}/books/{action=GetByCategory}",
    defaults: new { controller = "ConventionalBooks" });

// Author-specific route
app.MapControllerRoute(
    name: "author_books",
    pattern: "authors/{authorId:int}/books/{action=GetByAuthor}",
    defaults: new { controller = "ConventionalBooks" });

// Attribute-routed controllers
app.MapControllers();
```

## 🎯 **Controller Examples**

### 1. Conventional Routing Controller

**Controller**: `ConventionalBooksController`
**Routes Defined In**: `Program.cs`

| Endpoint | Route Pattern | Example |
|----------|---------------|---------|
| Get all books | `/api/{controller}` | `/api/ConventionalBooks` |
| Get book by ID | `/api/{controller}/Get/{id}` | `/api/ConventionalBooks/Get/1` |
| Get by category | `/categories/{categoryId}/books` | `/categories/1/books` |
| Get by author | `/authors/{authorId}/books` | `/authors/1/books` |
| Search books | `/api/{controller}/Search` | `/api/ConventionalBooks/Search?query=foundation` |

### 2. Attribute Routing Controller

**Controller**: `AttributeRoutingController`  
**Routes Defined In**: Controller attributes

| Endpoint | Route Attribute | Example |
|----------|----------------|---------|
| Basic route | `[HttpGet("attribute-demo/basic")]` | `/attribute-demo/basic` |
| With parameters | `[HttpGet("books-attr/{id:int}")]` | `/books-attr/123` |
| Complex constraints | `[HttpGet("books-attr/{categoryId:int:min(1)}/author/{authorId:int:min(1)}")]` | `/books-attr/1/author/2` |
| Optional parameters | `[HttpGet("books-attr/search/{query}/page/{page:int?}")]` | `/books-attr/search/scifi/page/2` |
| Regex constraints | `[HttpGet("books-attr/isbn/{isbn:regex(...)}")]` | `/books-attr/isbn/123-1234567890` |

### 3. Hybrid Routing Controller

**Controller**: `HybridRoutingController`
**Routes**: Mix of conventional and attribute

| Endpoint | Routing Type | Example |
|----------|-------------|---------|
| Index | Conventional | `/api/HybridRouting` |
| Custom endpoint | Attribute | `/hybrid/custom-endpoint` |
| Get by ID (conventional) | Conventional | `/api/HybridRouting/GetById/123` |
| Get by ID (attribute) | Attribute | `/hybrid/items/123` |

### 4. Advanced Routing Controller

**Controller**: `AdvancedRoutingController`
**Features**: Constraints, parameters, URL generation

| Feature | Example Route | Description |
|---------|---------------|-------------|
| Range constraint | `/advanced-routing/age/{age:int:range(1,120)}` | Age between 1-120 |
| Length constraint | `/advanced-routing/username/{username:length(3,20)}` | Username 3-20 chars |
| GUID constraint | `/advanced-routing/guid/{id:guid}` | Valid GUID required |
| DateTime constraint | `/advanced-routing/date/{date:datetime}` | Valid DateTime |
| Regex constraint | `/advanced-routing/isbn/{isbn:regex(...)}` | Custom ISBN pattern |
| Catch-all | `/advanced-routing/files/{*filepath}` | Captures full file paths |

## 🔧 **Route Constraints Reference**

### Built-in Constraints

| Constraint | Example | Description |
|------------|---------|-------------|
| `int` | `{id:int}` | Matches integer values |
| `bool` | `{flag:bool}` | Matches true/false |
| `datetime` | `{date:datetime}` | Matches DateTime values |
| `decimal` | `{price:decimal}` | Matches decimal values |
| `double` | `{weight:double}` | Matches double values |
| `float` | `{ratio:float}` | Matches float values |
| `guid` | `{id:guid}` | Matches GUID values |
| `long` | `{id:long}` | Matches long values |

### Range and Length Constraints

| Constraint | Example | Description |
|------------|---------|-------------|
| `min(value)` | `{id:int:min(1)}` | Minimum value constraint |
| `max(value)` | `{id:int:max(100)}` | Maximum value constraint |
| `range(min,max)` | `{age:int:range(18,65)}` | Value range constraint |
| `length(min,max)` | `{name:length(2,50)}` | String length constraint |
| `minlength(value)` | `{name:minlength(3)}` | Minimum string length |
| `maxlength(value)` | `{name:maxlength(20)}` | Maximum string length |

### Pattern Constraints

| Constraint | Example | Description |
|------------|---------|-------------|
| `regex(pattern)` | `{isbn:regex(^\\d{3}-\\d{10}$)}` | Regular expression pattern |
| `alpha` | `{name:alpha}` | Letters only |
| `required` | `{id:required}` | Non-empty value required |

## 🚀 **Testing the Routes**

### Using Browser/HTTP Client

```bash
# Conventional Routes
GET /api/ConventionalBooks
GET /api/ConventionalBooks/Get/1
GET /categories/1/books
GET /authors/2/books
GET /api/ConventionalBooks/Search?query=foundation

# Attribute Routes  
GET /attribute-demo/basic
GET /books-attr/123
GET /books-attr/1/author/2
GET /books-attr/search/scifi/page/2
POST /hybrid/categories/1/items

# Advanced Routes
GET /advanced-routing/age/25
GET /advanced-routing/username/john
GET /advanced-routing/guid/12345678-1234-1234-1234-123456789abc
GET /advanced-routing/date/2023-12-25
GET /advanced-routing/price/29.99
GET /advanced-routing/files/documents/report.pdf
```

### Route Testing Endpoints

- **Route Debug Info**: `/advanced-routing/debug`
- **Route Comparison**: `/hybrid/route-info`
- **All Available Routes**: Check controller actions for comprehensive lists

## 📊 **Routing Best Practices**

### When to Use Conventional Routing
- ✅ Traditional MVC applications
- ✅ Simple, predictable URL patterns
- ✅ RESTful APIs with standard patterns
- ✅ When you want centralized route configuration

### When to Use Attribute Routing
- ✅ Complex route patterns
- ✅ Action-specific routing requirements
- ✅ API versioning
- ✅ When you need fine-grained control
- ✅ Multiple routes to the same action

### Hybrid Approach Benefits
- ✅ Flexibility for different scenarios
- ✅ Gradual migration from conventional to attribute
- ✅ Mix simple and complex routing needs
- ✅ Support for legacy and modern patterns

## 🔍 **Route Resolution Order**

1. **Attribute routes** are evaluated first
2. **Conventional routes** are evaluated if no attribute route matches
3. More specific routes take precedence over generic ones
4. Routes are evaluated in the order they're defined

## 💡 **Advanced Features**

### URL Generation
```csharp
// Using named routes
var url = Url.Link("GetProduct", new { id = 123 });

// Using action names
var url = Url.Action("GetById", "Books", new { id = 123 });

// In views/responses
var editUrl = Url.Action(nameof(EditProduct), new { id });
```

### Route Values and Data
```csharp
// Access route data in actions
var routeData = HttpContext.GetRouteData();
var controller = routeData.Values["controller"];
var action = routeData.Values["action"];
```

### Custom Route Constraints
```csharp
// You can create custom route constraints by implementing IRouteConstraint
public class CustomConstraint : IRouteConstraint
{
    public bool Match(HttpContext httpContext, IRouter route, string routeKey, 
                     RouteValueDictionary values, RouteDirection routeDirection)
    {
        // Custom validation logic
        return true;
    }
}
```

## 🧪 **Testing Your Routes**

1. **Interactive Testing**: Use the test page at `/cors-test.html`
2. **Debug Endpoint**: Visit `/advanced-routing/debug` for route information
3. **Route Info**: Check `/hybrid/route-info` for current route details
4. **HTTP Clients**: Use Postman, curl, or browser for manual testing

## 📖 **Summary**

This implementation demonstrates:
- ✅ **Conventional routing** with custom patterns
- ✅ **Attribute routing** with various constraints  
- ✅ **Hybrid approach** mixing both techniques
- ✅ **Advanced constraints** and parameters
- ✅ **URL generation** and route debugging
- ✅ **Best practices** for different scenarios

The routing system provides maximum flexibility while maintaining clean, understandable URL patterns for your bookstore API.