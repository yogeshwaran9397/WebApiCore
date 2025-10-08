# Bookstore Web API

A comprehensive .NET 9 Web API for managing a bookstore with full CRUD operations, CORS support, and RESTful endpoints.

## 🏗️ **Architecture Overview**

### Models
- **Book**: Core entity with title, ISBN, price, stock, and relationships
- **Author**: Author information with biography and nationality
- **Category**: Book categorization with descriptions
- **Customer**: Customer details with address information
- **Order**: Order management with status tracking
- **OrderItem**: Line items for orders

### Controllers
- **BooksController**: Complete book management with search and pagination
- **AuthorsController**: Author management with book relationships
- **CategoriesController**: Category management with statistics
- **CorsExampleController**: CORS demonstrations with bookstore examples

## 📚 **API Endpoints**

### Books API (`/api/books`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books` | Get paginated books list |
| GET | `/api/books/{id}` | Get specific book by ID |
| POST | `/api/books` | Create new book |
| PUT | `/api/books/{id}` | Update existing book |
| DELETE | `/api/books/{id}` | Delete book |
| GET | `/api/books/search?title={title}&author={author}&isbn={isbn}` | Search books |

**Sample Book Response:**
```json
{
  "id": 1,
  "title": "Foundation",
  "isbn": "978-0553293357",
  "description": "A science fiction novel about psychohistory",
  "price": 15.99,
  "stockQuantity": 50,
  "publishedDate": "1951-05-01T00:00:00",
  "pages": 244,
  "language": "English",
  "publisher": "Gnome Press",
  "authorId": 1,
  "categoryId": 3,
  "author": {
    "id": 1,
    "firstName": "Isaac",
    "lastName": "Asimov",
    "fullName": "Isaac Asimov"
  },
  "category": {
    "id": 3,
    "name": "Science Fiction"
  }
}
```

### Authors API (`/api/authors`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/authors` | Get all authors |
| GET | `/api/authors/{id}` | Get specific author |
| POST | `/api/authors` | Create new author |
| PUT | `/api/authors/{id}` | Update author |
| DELETE | `/api/authors/{id}` | Delete author |
| GET | `/api/authors/search?name={name}&nationality={nationality}` | Search authors |
| GET | `/api/authors/{id}/books` | Get author's books |

### Categories API (`/api/categories`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get active categories |
| GET | `/api/categories/{id}` | Get specific category |
| POST | `/api/categories` | Create new category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |
| GET | `/api/categories/{id}/books` | Get category books |
| GET | `/api/categories/stats` | Get category statistics |

### CORS Examples (`/api/corsexample`)

| Method | Endpoint | Description | CORS Policy |
|--------|----------|-------------|-------------|
| GET | `/global` | Bookstore data with global policy | AllowAll |
| GET | `/specific` | Specific origins policy | SpecificOrigins |
| GET | `/restrictive` | Restrictive policy | RestrictivePolicy |
| POST | `/book-order` | Book order with CORS | SpecificOrigins |
| GET | `/no-cors` | CORS disabled | None |
| PUT | `/preflight-test` | Preflight request test | RestrictivePolicy |

## 🔧 **Sample Data**

The API comes pre-seeded with sample data:

### Authors
- Isaac Asimov (Science Fiction)
- Agatha Christie (Mystery)
- Stephen King (Horror)
- J.K. Rowling (Fantasy)
- George Orwell (Dystopian Fiction)

### Categories
- Fiction
- Non-Fiction
- Science Fiction
- Mystery
- Biography
- Fantasy
- Horror

### Books
- Foundation by Isaac Asimov
- Murder on the Orient Express by Agatha Christie
- The Shining by Stephen King
- Harry Potter and the Philosopher's Stone by J.K. Rowling
- 1984 by George Orwell

## 🌐 **CORS Configuration**

Three CORS policies are configured:

1. **AllowAll** (Development): Allows any origin, method, and header
2. **SpecificOrigins** (Recommended): Allows localhost:3000 with credentials
3. **RestrictivePolicy** (Production): Limited methods and headers only

## 🚀 **Getting Started**

### Prerequisites
- .NET 9 SDK
- Any REST client (Postman, curl, browser)

### Running the API
```bash
# Clone and navigate to project
cd WebCoreAPI

# Build the project
dotnet build

# Run the application
dotnet run

# API will be available at:
# http://localhost:5274
# https://localhost:7274
```

### Testing the API

1. **Interactive Test Page**: `http://localhost:5274/cors-test.html`
2. **OpenAPI/Swagger**: Available in development mode
3. **Manual Testing**: Use any HTTP client

## 📝 **Sample Requests**

### Create a New Book
```bash
POST /api/books
Content-Type: application/json

{
  "title": "Dune",
  "isbn": "978-0441013593",
  "description": "Science fiction novel about desert planet Arrakis",
  "price": 16.99,
  "stockQuantity": 30,
  "publishedDate": "1965-08-01",
  "pages": 688,
  "language": "English",
  "publisher": "Chilton Books",
  "authorId": 1,
  "categoryId": 3
}
```

### Search Books
```bash
GET /api/books/search?title=foundation&author=asimov
```

### Get Books with Pagination
```bash
GET /api/books?page=1&pageSize=5
```

### Create a New Author
```bash
POST /api/authors
Content-Type: application/json

{
  "firstName": "Frank",
  "lastName": "Herbert",
  "email": "frank.herbert@example.com",
  "biography": "American science fiction author best known for Dune",
  "dateOfBirth": "1920-10-08",
  "nationality": "American"
}
```

### Test CORS with Book Order
```bash
POST /api/corsexample/book-order
Content-Type: application/json
Origin: http://localhost:3000

{
  "customerName": "Jane Smith",
  "bookTitle": "Foundation",
  "quantity": 3,
  "price": 15.99
}
```

## 🔒 **Security Features**

- **CORS**: Configurable cross-origin resource sharing
- **Input Validation**: Model validation on all endpoints
- **Error Handling**: Consistent error responses
- **Logging**: Comprehensive request logging

## 🛠️ **Development Notes**

### In-Memory Storage
The current implementation uses in-memory collections for simplicity. For production:
- Replace with Entity Framework Core
- Add a proper database (SQL Server, PostgreSQL, etc.)
- Implement repositories and services

### Extending the API
To add new features:
1. Create new models in `/Models`
2. Add controllers in `/Controllers`
3. Update CORS policies if needed
4. Add appropriate validation

### CORS Best Practices
- Use `SpecificOrigins` or `RestrictivePolicy` in production
- Never use `AllowAll` with credentials in production
- List specific frontend URLs in allowed origins
- Limit HTTP methods and headers to what's needed

## 📊 **API Response Formats**

### Success Response
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3
}
```

### Error Response
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Book with ID 999 not found"
}
```

## 🧪 **Testing**

The API includes a comprehensive test page at `/cors-test.html` that demonstrates:
- All CORS policies in action
- Bookstore API endpoints
- Cross-origin request handling
- Preflight request scenarios
- Error handling

Visit `http://localhost:5274/cors-test.html` to interact with all endpoints and see CORS in action!