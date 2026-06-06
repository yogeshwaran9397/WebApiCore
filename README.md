# WebApiCore - ASP.NET Core Web API Demonstration Project

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive ASP.NET Core 9.0 Web API demonstration project showcasing modern web API development patterns, authentication, authorization, API versioning, CORS configuration, routing strategies, and comprehensive error handling.

> 📚 **New to ASP.NET Core Web API?** Start with the **[Visual Learning Guide](LEARNING-GUIDE.md)** — every curriculum topic explained with analogies, diagrams, and memory aids, each mapped to a **live endpoint you can run in this project**.

## 🧪 **Learning Demo Endpoints**

These endpoints exist purely to make each concept tangible. Run the app and open **`/swagger`** to try them.

| Topic | Endpoint(s) | What it shows |
| ----- | ----------- | ------------- |
| DI Lifetimes | `GET /api/lifetimes` | Singleton vs Scoped vs Transient (call twice & compare GUIDs) |
| Status Codes | `GET /api/status-codes/{200,404,500,…}` | Every common HTTP status code |
| Model Binding | `/api/binding/{route,query,header,body,form}` | `[FromRoute/Query/Header/Body/Form]` + content negotiation |
| HTTP Methods | `/api/http-methods` | GET/POST/PUT/PATCH/DELETE/HEAD/OPTIONS (full CRUD) |
| Validation | `POST /api/validation/{data-annotations,fluent}` | Data Annotations vs FluentValidation |
| Caching | `GET /api/caching/{memory,response,no-cache}` | In-memory + HTTP response caching |
| Logging | `GET /api/logging/{all-levels,exception,scope}` | `ILogger` levels, exceptions, scopes |
| Filters | `GET /api/filters/{pipeline,exception}` | Resource/Action/Result/Exception filter pipeline |
| Security | `POST /api/security/{hash,encrypt,hmac}` | Password hashing, AES, HMAC |
| Swagger | `/swagger` | Interactive API docs |

## 🚀 **Features**

### Core API Features

- **📚 Bookstore API**: Complete CRUD operations for books, authors, and categories
- **🔐 JWT Authentication**: Secure token-based authentication with refresh tokens
- **🛡️ Multi-Layer Authorization**: Role-based, policy-based, and claims-based authorization
- **📋 API Versioning**: Multiple versioning strategies (URL, query string, header, media type)
- **🌐 CORS Support**: Flexible Cross-Origin Resource Sharing configuration
- **🛣️ Advanced Routing**: Conventional, attribute-based, and hybrid routing patterns
- **⚠️ Global Exception Handling**: Comprehensive error handling middleware
- **📊 Request/Response Logging**: Detailed logging for debugging and monitoring

### Technical Highlights

- **.NET 9**: Latest .NET framework with enhanced performance
- **OpenAPI/Swagger**: Interactive API documentation
- **In-Memory Data**: Seeded demo data for immediate testing
- **Comprehensive Models**: Rich domain models with relationships
- **Service Layer**: Clean architecture with service abstractions
- **Middleware Pipeline**: Custom middleware for cross-cutting concerns

## 🏗️ **Project Structure**

```
WebApiCore/
├── Controllers/                    # API Controllers
│   ├── BooksController.cs         # Book management endpoints
│   ├── AuthController.cs          # Authentication endpoints
│   ├── AuthorsController.cs       # Author management
│   ├── CategoriesController.cs    # Category management
│   ├── VersioningDemoController.cs # API versioning examples
│   ├── *RoutingController.cs      # Routing pattern examples
│   ├── *AuthController.cs         # Authorization examples
│   ├── CorsExampleController.cs   # CORS demonstrations
│   ├── ExceptionDemoController.cs # Exception handling examples
│   └── V1/, V2/                   # Versioned controllers
├── Models/                        # Data Models
│   ├── Book.cs                    # Book entity model
│   ├── Author.cs                  # Author entity model
│   ├── Category.cs                # Category entity model
│   ├── Customer.cs, Order.cs      # E-commerce models
│   └── Auth/                      # Authentication models
├── Services/                      # Business Logic Services
│   ├── JwtTokenService.cs         # JWT token management
│   └── UserService.cs             # User management service
├── Middleware/                    # Custom Middleware
│   └── GlobalExceptionHandlingMiddleware.cs
├── Authorization/                 # Custom Authorization
│   └── CustomRequirements.cs     # Policy-based auth requirements
├── Exceptions/                    # Custom Exception Classes
│   └── CustomExceptions.cs
└── wwwroot/                      # Static Files
    └── cors-test.html            # CORS testing page
```

## 📋 **Prerequisites**

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) or similar API testing tool (optional)

## 🚀 **Getting Started**

### 1. Clone the Repository

```bash
git clone https://github.com/yogesh-grl/WebApiCore.git
cd WebApiCore
```

### 2. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project WebCoreAPI
```

### 3. Access the API

- **API Base URL**: `https://localhost:7000` or `http://localhost:5000`
- **OpenAPI/Swagger**: `https://localhost:7000/openapi/v1.json`
- **Health Check**: `GET /api/books` (returns sample books)

## 🔐 **Authentication & Authorization**

### Quick Authentication Test

```bash
# Login with demo user
POST https://localhost:7000/api/v1/auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "admin123"
}

# Use the returned JWT token in subsequent requests
Authorization: Bearer <your-jwt-token>
```

### Demo Users

| Username    | Password       | Roles                | Security Level | Description              |
| ----------- | -------------- | -------------------- | -------------- | ------------------------ |
| `admin`   | `admin123`   | Admin, Manager, User | 5              | Full system access       |
| `manager` | `manager123` | Manager, User        | 3              | Management access        |
| `user`    | `user123`    | User                 | 1              | Standard user access     |
| `support` | `support123` | Support, User        | 2              | Customer support access  |
| `guest`   | `guest123`   | Guest                | 0              | Limited read-only access |

## 📚 **API Endpoints**

### Authentication Endpoints

| Method | Endpoint                  | Description                    | Auth Required |
| ------ | ------------------------- | ------------------------------ | ------------- |
| GET    | `/api/v1/auth/info`     | Get authentication system info | ❌            |
| POST   | `/api/v1/auth/login`    | Login with credentials         | ❌            |
| POST   | `/api/v1/auth/register` | Register new user              | ❌            |
| GET    | `/api/v1/auth/me`       | Get current user profile       | ✅            |
| POST   | `/api/v1/auth/logout`   | Logout current user            | ✅            |

### Bookstore API Endpoints

| Method | Endpoint              | Description          | Auth Required |
| ------ | --------------------- | -------------------- | ------------- |
| GET    | `/api/books`        | Get paginated books  | ❌            |
| GET    | `/api/books/{id}`   | Get book by ID       | ❌            |
| POST   | `/api/books`        | Create new book      | ✅            |
| PUT    | `/api/books/{id}`   | Update existing book | ✅            |
| DELETE | `/api/books/{id}`   | Delete book          | ✅            |
| GET    | `/api/books/search` | Search books         | ❌            |
| GET    | `/api/authors`      | Get all authors      | ❌            |
| GET    | `/api/categories`   | Get all categories   | ❌            |

### Authorization Demo Endpoints

| Method | Endpoint                            | Policy            | Description                  |
| ------ | ----------------------------------- | ----------------- | ---------------------------- |
| GET    | `/api/role-auth/admin-only`       | AdminOnly         | Requires Admin role          |
| GET    | `/api/role-auth/manager-or-admin` | ManagerOrAdmin    | Requires Manager or Admin    |
| GET    | `/api/claims-auth/high-security`  | HighSecurityLevel | Requires security level 3+   |
| GET    | `/api/policy-auth/it-department`  | ITDepartment      | Requires IT department claim |

### API Versioning Examples

| Method | Endpoint                        | Version | Description              |
| ------ | ------------------------------- | ------- | ------------------------ |
| GET    | `/api/v1/books`               | 1.0     | Version 1 books endpoint |
| GET    | `/api/v2/books`               | 2.0     | Version 2 books endpoint |
| GET    | `/api/books?version=1.0`      | 1.0     | Query string versioning  |
| GET    | `/api/books` (X-Version: 2.0) | 2.0     | Header-based versioning  |

## 🧪 **Testing the API**

### Sample API Calls

#### 1. Get All Books

```http
GET https://localhost:7000/api/books?page=1&pageSize=5
```

#### 2. Login and Get Token

```http
POST https://localhost:7000/api/v1/auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "admin123"
}
```

#### 3. Create a New Book (Authenticated)

```http
POST https://localhost:7000/api/books
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
    "title": "Clean Code",
    "isbn": "978-0132350884",
    "description": "A handbook of agile software craftsmanship",
    "price": 32.99,
    "stockQuantity": 25,
    "pages": 464,
    "publisher": "Prentice Hall",
    "authorId": 1,
    "categoryId": 2,
    "publishedDate": "2008-08-01"
}
```

#### 4. Test Authorization

```http
GET https://localhost:7000/api/role-auth/admin-only
Authorization: Bearer <admin-jwt-token>
```

## 🛣️ **Routing Strategies**

The API demonstrates multiple routing approaches:

1. **Conventional Routing**: `/api/{controller}/{action}/{id?}`
2. **Attribute Routing**: `[Route("api/books/{id}")]`
3. **Hybrid Routing**: Combined conventional and attribute routing
4. **Custom Routes**: Bookstore-specific routes like `/bookstore/books`
5. **Versioned Routes**: `/api/v{version:apiVersion}/books`

## 🌐 **CORS Configuration**

Multiple CORS policies configured:

- **AllowAll**: Development policy (allows all origins)
- **SpecificOrigins**: Production policy (specific allowed origins)
- **RestrictivePolicy**: Limited methods and headers

Test CORS with the included HTML page: `/wwwroot/cors-test.html`

## ⚠️ **Error Handling**

Comprehensive global exception handling with:

- **Structured Error Responses**: Consistent error format
- **Detailed Logging**: Request tracking and correlation IDs
- **Environment-Specific Details**: Stack traces in development
- **Custom Exception Types**: Domain-specific exceptions
- **HTTP Status Code Mapping**: Proper status codes for different errors

## 📖 **Documentation**

Detailed documentation available in the `/doc` folder:

- [Authentication &amp; Authorization Guide](doc/AUTHENTICATION-GUIDE.md)
- [API Versioning Guide](doc/API-VERSIONING-GUIDE.md)
- [Routing Patterns Guide](doc/ROUTING-GUIDE.md)
- [CORS Configuration Guide](doc/CORS-README.md)
- [Exception Handling Guide](doc/EXCEPTION-HANDLING-GUIDE.md)
- [Bookstore API Details](doc/BOOKSTORE-API-README.md)

## 🔧 **Configuration**

### JWT Configuration

Configure JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatShouldBeAtLeast32Characters!",
    "Issuer": "WebCoreAPI",
    "Audience": "WebCoreAPIUsers",
    "ExpiryHours": 24
  }
}
```

### CORS Configuration

Modify CORS policies in `Program.cs` for production deployment.

## 🚀 **Deployment**

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["WebCoreAPI/WebCoreAPI.csproj", "WebCoreAPI/"]
RUN dotnet restore "WebCoreAPI/WebCoreAPI.csproj"
COPY . .
WORKDIR "/src/WebCoreAPI"
RUN dotnet build "WebCoreAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebCoreAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCoreAPI.dll"]
```

### IIS Deployment

1. Publish the application: `dotnet publish -c Release`
2. Copy published files to IIS wwwroot
3. Configure IIS with ASP.NET Core Module
4. Update connection strings and JWT settings

## 🤝 **Contributing**

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 🏷️ **Tags**

`asp.net-core` `web-api` `jwt-authentication` `authorization` `api-versioning` `cors` `routing` `exception-handling` `swagger` `openapi` `dotnet-9` `rest-api` `bookstore-api` `demo-project`

---

⭐ **Star this repository** if you find it helpful for learning ASP.NET Core Web API development!
