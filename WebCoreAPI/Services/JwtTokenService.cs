using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebCoreAPI.Models.Auth;

namespace WebCoreAPI.Services;

/// <summary>
/// JWT Token Service for generating and validating JWT tokens
/// </summary>
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generate JWT token for a user
    /// </summary>
    public string GenerateToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32Characters!");
        var tokenHandler = new JwtSecurityTokenHandler();

        // Create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("department", user.Department),
            new("security_level", user.SecurityLevel.ToString()),
            new("region", user.Region),
            new("user_id", user.Id.ToString()),
            new("full_name", $"{user.FirstName} {user.LastName}")
        };

        // Add roles as claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        foreach (var claim in user.Claims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        // Add permissions as claims
        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24), // Token valid for 24 hours
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"] ?? "WebCoreAPI",
            Audience = _configuration["Jwt:Audience"] ?? "WebCoreAPIUsers"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Generated JWT token for user {Username} with {ClaimCount} claims", 
            user.Username, claims.Count);

        return tokenString;
    }

    /// <summary>
    /// Generate refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Validate JWT token and extract claims
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32Characters!");
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "WebCoreAPI",
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"] ?? "WebCoreAPIUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token validation failed: {Error}", ex.Message);
            return null;
        }
    }
}