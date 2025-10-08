# API Versioning Implementation Guide

A comprehensive demonstration of API versioning techniques in ASP.NET Core Web API, showcasing multiple versioning strategies and best practices.

## 🔄 **Versioning Overview**

This implementation supports **four different versioning methods** that can be used individually or in combination:

1. **URL Path Versioning** (e.g., `/api/v1/books`, `/api/v2/books`)
2. **Query String Versioning** (e.g., `/api/books?version=2.0`)
3. **Header Versioning** (e.g., `X-Version: 2.0`)
4. **Media Type Versioning** (e.g., `Accept: application/json;version=2.0`)

## 📦 **Package Dependencies**

```xml
<PackageReference Include="Asp.Versioning.Mvc" Version="8.0.0" />
<PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.0.0" />
```

## ⚙️ **Configuration (Program.cs)**

```csharp
builder.Services.AddApiVersioning(options =>
{
    // Default version when none is specified
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // Configure how versions are read from requests
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),        // /api/v1/books
        new QueryStringApiVersionReader("version"), // ?version=1.0
        new HeaderApiVersionReader("X-Version"),    // X-Version: 1.0
        new MediaTypeApiVersionReader("version")    // Accept: application/json;version=1.0
    );
    
    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
}).AddMvc().AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
```

## 📚 **Available API Versions**

### Version 1.0 - Basic Implementation
- **Focus**: Simple CRUD operations with basic functionality
- **Features**:
  - Basic book listing (no pagination)
  - Simple search (title and author only)
  - Basic error handling
  - Minimal data structure
- **Target Audience**: Initial API consumers, simple integrations

### Version 2.0 - Enhanced Implementation  
- **Focus**: Advanced features with rich functionality
- **Features**:
  - Paginated responses with metadata
  - Advanced filtering (genre, price, rating)
  - Multi-field search with relevance scoring
  - Detailed error responses with suggestions
  - Rich data models with relationships
  - Genre statistics and analytics
- **Target Audience**: Advanced integrations, modern applications

## 🛠️ **API Endpoints by Version**

### Books API V1 (`/api/v1/books`)

| Endpoint | Method | Description |
|----------|---------|-------------|
| `/api/v1/books` | GET | Get all books (simple list) |
| `/api/v1/books/{id}` | GET | Get book by ID (basic info) |
| `/api/v1/books/search?query={query}` | GET | Simple search (title/author) |
| `/api/v1/books` | POST | Create book (basic) |
| `/api/v1/books/version-info` | GET | V1 API information |

### Books API V2 (`/api/v2/books`)

| Endpoint | Method | Description |
|----------|---------|-------------|
| `/api/v2/books` | GET | Get books with pagination & filtering |
| `/api/v2/books/{id}` | GET | Get detailed book with related data |
| `/api/v2/books/search` | GET | Advanced multi-field search |
| `/api/v2/books/genres` | GET | Genre statistics |
| `/api/v2/books/version-info` | GET | V2 API information |

### Versioning Demo API (`/api/versioning-demo`)

| Endpoint | Description | Versions |
|----------|-------------|----------|
| `/url-path` | URL path versioning demo | 1.0, 2.0 |
| `/query-string` | Query string versioning demo | 1.0, 2.0 |
| `/header` | Header versioning demo | 1.0, 2.0 |
| `/media-type` | Media type versioning demo | 1.0, 2.0 |
| `/combined` | All versioning methods demo | 1.0, 2.0 |
| `/info` | Versioning information | N/A |

## 🎯 **Versioning Methods Explained**

### 1. URL Path Versioning 

**Format**: `/api/v{version}/{resource}`

**Examples**:
```bash
GET /api/v1/books           # Version 1.0
GET /api/v2/books           # Version 2.0
```

**Pros**:
- ✅ Very explicit and visible
- ✅ Easy to cache
- ✅ RESTful approach
- ✅ Simple routing

**Cons**:
- ❌ URL proliferation
- ❌ Harder to maintain multiple versions

### 2. Query String Versioning

**Format**: `/api/{resource}?version={version}`

**Examples**:
```bash
GET /api/books?version=1.0  # Version 1.0  
GET /api/books?version=2.0  # Version 2.0
```

**Pros**:
- ✅ Easy to implement
- ✅ Optional parameter (fallback to default)
- ✅ Flexible and simple

**Cons**:
- ❌ Can be overlooked by developers
- ❌ Not strictly RESTful

### 3. Header Versioning

**Format**: `X-Version: {version}`

**Examples**:
```bash
curl -H "X-Version: 1.0" /api/books  # Version 1.0
curl -H "X-Version: 2.0" /api/books  # Version 2.0
```

**Pros**:
- ✅ Clean URLs
- ✅ Out-of-band versioning
- ✅ Flexible and extensible

**Cons**:
- ❌ Hidden from URL
- ❌ Requires proper documentation
- ❌ May be stripped by proxies

### 4. Media Type Versioning

**Format**: `Accept: application/json;version={version}`

**Examples**:
```bash
curl -H "Accept: application/json;version=1.0" /api/books  # Version 1.0
curl -H "Accept: application/json;version=2.0" /api/books  # Version 2.0
```

**Pros**:
- ✅ HTTP standard compliant
- ✅ Content negotiation support
- ✅ Precise and formal

**Cons**:
- ❌ More complex to implement
- ❌ Limited client support
- ❌ Can be confusing for developers

## 🔍 **Testing Different Versioning Methods**

### Using cURL

```bash
# URL Path Versioning
curl http://localhost:5274/api/v1/books
curl http://localhost:5274/api/v2/books

# Query String Versioning  
curl "http://localhost:5274/api/books?version=1.0"
curl "http://localhost:5274/api/books?version=2.0"

# Header Versioning
curl -H "X-Version: 1.0" http://localhost:5274/api/versioning-demo/header
curl -H "X-Version: 2.0" http://localhost:5274/api/versioning-demo/header

# Media Type Versioning
curl -H "Accept: application/json;version=1.0" http://localhost:5274/api/versioning-demo/media-type
curl -H "Accept: application/json;version=2.0" http://localhost:5274/api/versioning-demo/media-type
```

### Using JavaScript/Fetch

```javascript
// URL Path Versioning
fetch('/api/v2/books');

// Query String Versioning
fetch('/api/books?version=2.0');

// Header Versioning  
fetch('/api/versioning-demo/header', {
    headers: { 'X-Version': '2.0' }
});

// Media Type Versioning
fetch('/api/versioning-demo/media-type', {
    headers: { 'Accept': 'application/json;version=2.0' }
});
```

## 📊 **Version Resolution Priority**

When multiple versioning methods are present, the resolution priority is:

1. **URL Path** (highest priority)
2. **Query String**
3. **Header**
4. **Media Type** (lowest priority)

**Example**: If a request has both `/api/v1/books` and `X-Version: 2.0`, version 1.0 will be used.

## 🎨 **Response Differences Between Versions**

### V1.0 Response Example:
```json
{
  "version": "1.0",
  "message": "Books retrieved using API Version 1.0",
  "features": ["Basic book listing", "Simple data structure", "No pagination"],
  "data": [
    {
      "id": 1,
      "title": "Foundation",
      "author": "Isaac Asimov", 
      "price": 15.99,
      "year": 1951
    }
  ],
  "count": 5
}
```

### V2.0 Response Example:
```json
{
  "version": "2.0",
  "data": [
    {
      "id": 1,
      "title": "Foundation",
      "author": {
        "id": 1,
        "name": "Isaac Asimov",
        "nationality": "American"
      },
      "price": 15.99,
      "publishedYear": 1951,
      "genre": "Science Fiction",
      "pages": 244,
      "isbn": "978-0553293357",
      "rating": 4.5,
      "stock": 50,
      "description": "The first novel in Isaac Asimov's Foundation trilogy"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalItems": 6,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "metadata": {
    "message": "Books retrieved using API Version 2.0",
    "features": ["Pagination support", "Genre filtering", "Advanced sorting"]
  }
}
```

## 🚦 **Migration Strategy**

### From V1 to V2:
1. **Additive Changes**: New fields and endpoints added
2. **Enhanced Responses**: Richer data structure with metadata
3. **Backward Compatibility**: V1 endpoints remain functional
4. **Deprecation Timeline**: V1 supported for 12 months after V2 release

### Breaking Changes in V2:
- Response structure includes pagination wrapper
- Error responses have structured format
- Book model has enhanced fields (author object instead of string)
- Default page size applied to listing endpoints

## 🏗️ **Best Practices Implemented**

### 1. **Semantic Versioning**
- Major versions for breaking changes
- Minor versions for new features
- Patch versions for bug fixes

### 2. **Backward Compatibility**
- V1 endpoints remain available
- Graceful degradation for missing fields
- Clear migration paths

### 3. **Documentation**
- Version-specific endpoint documentation
- Breaking changes clearly marked
- Migration guides provided

### 4. **Error Handling**
- Version-aware error responses
- Consistent error formats within versions
- Helpful error messages with suggestions

### 5. **Performance Considerations**
- Version-specific optimizations
- Efficient data structures per version
- Appropriate caching strategies

## 🧪 **Testing Your Versioned API**

### 1. **Interactive Testing**
Visit `/cors-test.html` for browser-based testing of all versioning methods.

### 2. **Version Information Endpoints**
- `/api/v1/books/version-info` - V1 capabilities and limitations
- `/api/v2/books/version-info` - V2 features and improvements
- `/api/versioning-demo/info` - Complete versioning guide

### 3. **Automated Testing**
```bash
# Test all versioning methods
curl http://localhost:5274/api/versioning-demo/combined
```

## 📈 **Monitoring and Analytics**

### Version Usage Tracking:
- Request logs include version information
- Usage analytics per version
- Deprecation timeline monitoring
- Client migration tracking

### Health Checks:
- Version-specific health endpoints
- Performance metrics per version
- Error rate monitoring by version

## 🔮 **Future Versioning Strategy**

### Planned Versions:
- **V2.1**: Minor enhancements, additional filters
- **V3.0**: GraphQL support, real-time updates
- **V4.0**: Microservices architecture migration

### Deprecation Policy:
- Minimum 12 months support for major versions
- 6 months advance notice for deprecation
- Clear migration documentation
- Automated migration tools where possible

This comprehensive versioning implementation provides maximum flexibility while maintaining stability and clear upgrade paths for API consumers.