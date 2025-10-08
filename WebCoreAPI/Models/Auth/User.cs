namespace WebCoreAPI.Models.Auth;

/// <summary>
/// User model for authentication and authorization
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // In real apps, use hashed passwords
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Claims { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Additional properties for authorization examples
    public string Department { get; set; } = string.Empty;
    public int SecurityLevel { get; set; } = 1; // 1-5 security clearance level
    public List<string> Permissions { get; set; } = new();
    public string Region { get; set; } = string.Empty;
}

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Register request model
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// JWT authentication response
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expires { get; set; }
    public User User { get; set; } = new();
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}