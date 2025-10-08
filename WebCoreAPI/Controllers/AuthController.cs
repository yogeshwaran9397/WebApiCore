using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCoreAPI.Models.Auth;
using WebCoreAPI.Services;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Authentication Controller - Handles login, registration, and token management
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserService userService,
        JwtTokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for username: {Username}", request.Username);

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await _userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddHours(24),
                RefreshToken = refreshToken,
                User = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Roles = user.Roles,
                    Permissions = user.Permissions,
                    SecurityLevel = user.SecurityLevel,
                    Region = user.Region,
                    LastLoginAt = user.LastLoginAt
                }
            };

            _logger.LogInformation("Successful login for user: {Username}", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error during authentication" });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration information</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Username, password, and email are required" });
            }

            var user = await _userService.RegisterAsync(request);
            var token = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var response = new AuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddHours(24),
                RefreshToken = refreshToken,
                User = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Roles = user.Roles,
                    Permissions = user.Permissions,
                    SecurityLevel = user.SecurityLevel,
                    Region = user.Region,
                    CreatedAt = user.CreatedAt
                }
            };

            _logger.LogInformation("Successful registration for user: {Username}", request.Username);
            return CreatedAtAction(nameof(GetCurrentUser), new { }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Registration failed for username {Username}: {Message}", request.Username, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error during registration" });
        }
    }

    /// <summary>
    /// Get current user information (requires authentication)
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Don't return password
            var userResponse = new User
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Roles = user.Roles,
                Permissions = user.Permissions,
                SecurityLevel = user.SecurityLevel,
                Region = user.Region,
                Claims = user.Claims,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };

            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Logout (invalidate token)
    /// Note: In a real application, you would maintain a blacklist of invalidated tokens
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        var username = User.Identity?.Name;
        _logger.LogInformation("User {Username} logged out", username);
        
        return Ok(new 
        { 
            message = "Logged out successfully",
            timestamp = DateTime.UtcNow,
            note = "In production, implement token blacklisting for enhanced security"
        });
    }

    /// <summary>
    /// Get authentication information and available demo users
    /// </summary>
    /// <returns>Authentication system information</returns>
    [HttpGet("info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAuthInfo()
    {
        var demoUsers = new[]
        {
            new { Username = "admin", Password = "admin123", Roles = new[] { "Admin", "User", "Manager" }, Description = "Full system access" },
            new { Username = "manager", Password = "manager123", Roles = new[] { "Manager", "User" }, Description = "Management access" },
            new { Username = "user", Password = "user123", Roles = new[] { "User" }, Description = "Standard user access" },
            new { Username = "support", Password = "support123", Roles = new[] { "Support", "User" }, Description = "Customer support access" },
            new { Username = "guest", Password = "guest123", Roles = new[] { "Guest" }, Description = "Limited read-only access" }
        };

        var authPolicies = new[]
        {
            new { Policy = "AdminOnly", Description = "Requires Admin role" },
            new { Policy = "ManagerOrAdmin", Description = "Requires Manager or Admin role" },
            new { Policy = "UserOrAbove", Description = "Requires User, Manager, or Admin role" },
            new { Policy = "HighSecurityLevel", Description = "Requires security level 3, 4, or 5" },
            new { Policy = "ITDepartment", Description = "Requires IT department" },
            new { Policy = "SecurityLevel3", Description = "Requires minimum security level 3" },
            new { Policy = "CanReadUsers", Description = "Requires 'users.read' permission" },
            new { Policy = "SystemAdministrator", Description = "Composite policy: Admin role + Security Level 4 + System permissions" }
        };

        return Ok(new
        {
            AuthenticationMethod = "JWT Bearer Token",
            TokenExpiration = "24 hours",
            SupportedVersions = new[] { "1.0", "2.0" },
            DemoUsers = demoUsers,
            AvailablePolicies = authPolicies,
            Usage = new
            {
                Login = "POST /api/v1/auth/login",
                Register = "POST /api/v1/auth/register",
                GetProfile = "GET /api/v1/auth/me (requires Bearer token)",
                AuthHeader = "Authorization: Bearer <your-jwt-token>"
            },
            Examples = new
            {
                LoginRequest = new LoginRequest { Username = "admin", Password = "admin123" },
                RegisterRequest = new RegisterRequest 
                { 
                    Username = "newuser", 
                    Password = "password123", 
                    Email = "newuser@example.com",
                    FirstName = "New",
                    LastName = "User",
                    Department = "Sales"
                }
            }
        });
    }
}