# CORS Implementation Example

This .NET Core Web API project demonstrates various CORS (Cross-Origin Resource Sharing) configurations and scenarios.

## CORS Policies Configured

### 1. AllowAll Policy (Development Only)
- **Usage**: Global policy applied to all endpoints
- **Configuration**: Allows any origin, method, and header
- **Security**: ⚠️ Use only in development environments

### 2. SpecificOrigins Policy 
- **Usage**: Applied to specific endpoints using `[EnableCors("SpecificOrigins")]`
- **Allowed Origins**: 
  - `http://localhost:3000`
  - `https://localhost:3000` 
  - `http://127.0.0.1:3000`
- **Configuration**: Any method, any header, credentials allowed
- **Security**: ✅ Recommended for production with your actual frontend URLs

### 3. RestrictivePolicy
- **Usage**: Most secure policy for sensitive endpoints
- **Allowed Origins**: `http://localhost:3000`, `https://myapp.com`
- **Allowed Methods**: GET, POST, PUT only
- **Allowed Headers**: Content-Type, Authorization only
- **Configuration**: Credentials allowed
- **Security**: ✅ Most secure option

## API Endpoints

### CORS Example Controller (`/api/corsexample`)

1. **GET `/global`** - Uses global CORS policy
2. **GET `/specific`** - Uses SpecificOrigins policy
3. **GET `/restrictive`** - Uses RestrictivePolicy
4. **POST `/data`** - Accepts JSON data with SpecificOrigins policy
5. **GET `/no-cors`** - CORS disabled (same-origin only)
6. **OPTIONS `/preflight-test`** - Preflight endpoint
7. **PUT `/preflight-test`** - PUT request requiring preflight check

### Weather Controller (`/api/weather`)
- Standard weather forecast endpoints with global CORS policy

## Testing CORS

### Method 1: Use the Test Page
1. Run the API: `dotnet run`
2. Open browser to: `http://localhost:5274/cors-test.html`
3. Click the test buttons to see different CORS scenarios

### Method 2: Manual Testing with Browser DevTools
```javascript
// Test from browser console on any website (different origin)
fetch('http://localhost:5274/api/corsexample/global')
  .then(response => response.json())
  .then(data => console.log(data))
  .catch(error => console.error('CORS Error:', error));
```

### Method 3: Using curl
```bash
# Simple GET request
curl -H "Origin: http://localhost:3000" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: X-Requested-With" \
     -X OPTIONS \
     http://localhost:5274/api/corsexample/specific

# Actual request after preflight
curl -H "Origin: http://localhost:3000" \
     http://localhost:5274/api/corsexample/specific
```

## CORS Headers Explained

When CORS is properly configured, you'll see these response headers:

- `Access-Control-Allow-Origin`: Specifies allowed origins
- `Access-Control-Allow-Methods`: Lists allowed HTTP methods
- `Access-Control-Allow-Headers`: Lists allowed request headers
- `Access-Control-Allow-Credentials`: Indicates if credentials are allowed
- `Access-Control-Max-Age`: Preflight cache duration

## Security Best Practices

1. **Never use `AllowAnyOrigin()` in production**
2. **Be specific with allowed origins** - list exact URLs
3. **Limit HTTP methods** to only what's needed
4. **Restrict headers** to only required ones
5. **Use HTTPS** for production origins
6. **Consider credentials carefully** - only allow when necessary

## Running the Project

```bash
# Build the project
dotnet build

# Run the project
dotnet run

# The API will be available at:
# - HTTP: http://localhost:5274
# - HTTPS: https://localhost:7274

# Test page available at:
# http://localhost:5274/cors-test.html
```

## Common CORS Issues & Solutions

### Issue: "CORS policy blocked"
- **Cause**: Origin not in allowed list
- **Solution**: Add your frontend URL to the CORS policy

### Issue: "Preflight request failed"
- **Cause**: Complex requests require preflight OPTIONS request
- **Solution**: Ensure your policy allows the HTTP method and headers

### Issue: "Credentials not allowed"
- **Cause**: Using `AllowAnyOrigin()` with `AllowCredentials()`
- **Solution**: Use specific origins when allowing credentials

### Issue: "Method not allowed"
- **Cause**: HTTP method not in allowed list
- **Solution**: Add the method to `WithMethods()`