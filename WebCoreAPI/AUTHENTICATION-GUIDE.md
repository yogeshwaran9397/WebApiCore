# Authentication & Authorization Guide

## Overview

This .NET Core Web API implements comprehensive authentication and authorization using JWT tokens with three primary authorization approaches:

1. **Role-Based Authorization** - Access control based on user roles
2. **Policy-Based Authorization** - Custom policies with requirements and handlers  
3. **Claims-Based Authorization** - Authorization based on JWT token claims

## Quick Start

### 1. Authentication Endpoints

| Endpoint | Method | Description | Authentication |
|----------|--------|-------------|----------------|
| `/api/v1/auth/info` | GET | Get authentication system info | None |
| `/api/v1/auth/login` | POST | Login with credentials | None |
| `/api/v1/auth/register` | POST | Register new user | None |
| `/api/v1/auth/me` | GET | Get current user info | JWT Required |
| `/api/v1/auth/logout` | POST | Logout | JWT Required |

### 2. Demo Users (Username/Password)

| User | Password | Roles | Security Level | Department | Description |
|------|----------|-------|---------------|------------|-------------|
| `admin` | `admin123` | Admin, Manager, User | 5 | IT | Full system access |
| `manager` | `manager123` | Manager, User | 3 | Sales | Management access |
| `user` | `user123` | User | 1 | Sales | Standard user access |
| `support` | `support123` | Support, User | 2 | Customer Service | Support access |
| `guest` | `guest123` | Guest | 0 | None | Limited read access |

## Authentication Flow

### 1. Login Request
```json
POST /api/v1/auth/login
{
  "username": "admin",
  "password": "admin123"
}
```

### 2. Login Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires": "2025-10-07T12:00:00Z",
  "refreshToken": "guid-refresh-token",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@bookstore.com",
    "roles": ["Admin", "Manager", "User"],
    "permissions": ["users.read", "users.write", "users.delete", "..."],
    "securityLevel": 5,
    "department": "IT"
  }
}
```

### 3. Using JWT Token
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Authorization Approaches

### 1. Role-Based Authorization

#### Basic Role Requirements
```csharp
[Authorize(Roles = "Admin")]                    // Admin only
[Authorize(Roles = "Admin,Manager")]            // Admin OR Manager
[Authorize(Policy = "UserOrAbove")]             // User, Manager, OR Admin
```

#### Test Endpoints
- `/api/v1/role-demo/public` - No authentication required
- `/api/v1/role-demo/authenticated` - Any authenticated user
- `/api/v1/role-demo/admin-only` - Admin role required
- `/api/v1/role-demo/manager-or-admin` - Manager or Admin role
- `/api/v1/role-demo/user-or-above` - User, Manager, or Admin role

#### Role Hierarchy
```
Level 5: Admin     → Full system access
Level 4: Manager   → Management and team access  
Level 3: User      → Standard user operations
Level 2: Support   → Customer service tools
Level 1: Guest     → Limited read-only access
```

### 2. Policy-Based Authorization

#### Security Level Policies
```csharp
[Authorize(Policy = "SecurityLevel2")]   // Minimum security clearance: 2
[Authorize(Policy = "SecurityLevel3")]   // Minimum security clearance: 3
[Authorize(Policy = "SecurityLevel4")]   // Minimum security clearance: 4
[Authorize(Policy = "SecurityLevel5")]   // Maximum security clearance required
```

#### Department Policies
```csharp
[Authorize(Policy = "ITOrSalesDepartment")]        // IT or Sales department
[Authorize(Policy = "CustomerServiceDepartment")]  // Customer Service department
```

#### Permission Policies
```csharp
[Authorize(Policy = "CanReadUsers")]     // Requires 'users.read' permission
[Authorize(Policy = "CanWriteUsers")]    // Requires 'users.write' permission
[Authorize(Policy = "CanDeleteUsers")]   // Requires 'users.delete' permission
```

#### Composite Policies
```csharp
[Authorize(Policy = "HighLevelManager")]     // Security Level ≥ 3 + Admin/Manager role + specific permissions
[Authorize(Policy = "SystemAdministrator")] // Admin role + Security Level ≥ 4 + system.admin permission
```

#### Test Endpoints
- `/api/v1/policy-demo/security-level-2` - Security Level 2+ required
- `/api/v1/policy-demo/security-level-4` - Security Level 4+ required
- `/api/v1/policy-demo/it-department` - IT or Sales department required
- `/api/v1/policy-demo/user-management` - 'users.read' permission required
- `/api/v1/policy-demo/system-admin` - System Administrator composite policy

### 3. Claims-Based Authorization

#### Standard Claims
- `sub` (Subject) - User ID
- `name` - Username
- `email` - User email
- `role` - User roles (multiple values)

#### Custom Claims
- `department` - User's department
- `security_level` - Security clearance level (0-5)
- `region` - Geographic region
- `employee_id` - Employee identifier
- `hire_date` - Employment start date
- `salary_band` - Salary classification
- `permission` - Specific permissions (multiple values)

#### Claims-Based Policies
```csharp
[Authorize(Policy = "ITDepartment")]      // department = "IT"
[Authorize(Policy = "HighSecurityLevel")] // security_level ∈ {3, 4, 5}  
[Authorize(Policy = "ExecutiveAccess")]   // salary_band ∈ {"Executive", "Manager"}
```

#### Test Endpoints
- `/api/v1/claims-demo/my-claims` - View all user claims
- `/api/v1/claims-demo/it-department-only` - IT department claim required
- `/api/v1/claims-demo/high-security` - High security level claims
- `/api/v1/claims-demo/executive-access` - Executive salary band required
- `/api/v1/claims-demo/composite-claims` - Multiple claim validation

## Comprehensive Testing

### Authorization Demo Controller

The `/api/v1/auth-demo/` endpoints provide comprehensive testing of all authorization approaches:

- `/api/v1/auth-demo/overview` - System overview (no auth required)
- `/api/v1/auth-demo/token-info` - JWT token analysis (auth required)
- `/api/v1/auth-demo/role-matrix` - Role-based authorization matrix
- `/api/v1/auth-demo/policy-matrix` - Policy-based authorization matrix  
- `/api/v1/auth-demo/claims-matrix` - Claims-based authorization matrix
- `/api/v1/auth-demo/comprehensive-test` - All authorization approaches combined

### Version Differences

**Version 1.0**: Basic authorization testing
**Version 2.0**: Enhanced with additional metrics:
- Security analysis
- Compliance status
- Access patterns
- Risk assessment
- Audit trail information

## Testing Examples

### 1. Get Authentication Info
```bash
curl -X GET "http://localhost:5274/api/v1/auth/info"
```

### 2. Login as Admin
```bash
curl -X POST "http://localhost:5274/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

### 3. Access Protected Endpoint
```bash
curl -X GET "http://localhost:5274/api/v1/role-demo/admin-only" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Test Comprehensive Authorization
```bash
curl -X GET "http://localhost:5274/api/v2/auth-demo/comprehensive-test" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatShouldBeAtLeast32Characters!",
    "Issuer": "WebCoreAPI",
    "Audience": "WebCoreAPIUsers"
  }
}
```

### Custom Authorization Policies

The system includes these pre-configured policies:

#### Role-Based Policies
- `AdminOnly` - Requires Admin role
- `ManagerOrAdmin` - Requires Manager or Admin role
- `UserOrAbove` - Requires User, Manager, or Admin role
- `SupportAccess` - Requires Support, Manager, or Admin role

#### Claims-Based Policies  
- `HighSecurityLevel` - Requires security_level ∈ {3, 4, 5}
- `ITDepartment` - Requires department = "IT"
- `ExecutiveAccess` - Requires salary_band ∈ {"Executive", "Manager"}

#### Custom Requirement Policies
- `SecurityLevel2` through `SecurityLevel5` - Minimum security levels
- `ITOrSalesDepartment` - Department-based access
- `CanReadUsers`, `CanWriteUsers`, `CanDeleteUsers` - Permission-based access
- `NorthAmericaRegion`, `EuropeRegion` - Region-based access

#### Composite Policies
- `HighLevelManager` - Multiple requirements (role + security + permissions)
- `SystemAdministrator` - Admin role + high security + system permissions
- `SeniorStaff` - Authenticated + security level + department requirements

## Security Features

### JWT Token Security
- 24-hour token expiration
- Symmetric key signing (HMAC-SHA256)
- Issuer and audience validation
- Clock skew tolerance: 0 seconds

### Authorization Handlers
- `SecurityLevelHandler` - Validates minimum security clearance
- `DepartmentHandler` - Validates department membership
- `PermissionHandler` - Validates specific permissions
- `RegionHandler` - Validates regional access
- `CompositeHandler` - Validates multiple requirements simultaneously

### Logging & Auditing
- Authentication attempts logged
- Authorization decisions logged  
- Access patterns tracked
- Security events recorded

## Best Practices

### 1. Token Management
- Store tokens securely (not in localStorage for production)
- Implement token refresh mechanism
- Use HTTPS in production
- Implement token blacklisting for logout

### 2. Authorization Design
- Use least privilege principle
- Combine multiple authorization approaches for sensitive operations
- Implement proper error handling
- Log authorization decisions for auditing

### 3. Security Considerations
- Hash passwords in production (currently using plaintext for demo)
- Implement rate limiting for authentication endpoints
- Use strong JWT signing keys
- Regular security audits and updates

### 4. Performance
- Cache authorization decisions when appropriate
- Minimize database calls in authorization handlers
- Use efficient claim structures
- Consider authorization middleware for common checks

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden  
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden", 
  "status": 403
}
```

### Custom Error Response
```json
{
  "message": "Invalid username or password",
  "timestamp": "2025-10-06T12:00:00Z"
}
```

## Extending the System

### Adding New Roles
1. Update `UserService.InitializeUsers()` 
2. Add role to authorization policies in `Program.cs`
3. Create role-specific endpoints

### Adding New Claims
1. Update `JwtTokenService.GenerateToken()` 
2. Add claims to user models
3. Create claim-based policies

### Adding New Policies
1. Create custom requirement class implementing `IAuthorizationRequirement`
2. Create custom handler implementing `AuthorizationHandler<TRequirement>`
3. Register handler in `Program.cs`
4. Add policy configuration

This comprehensive authentication and authorization system provides a robust foundation for securing .NET Core Web APIs with multiple authorization strategies and extensive customization options.