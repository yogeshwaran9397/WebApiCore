using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Role-Based Authorization Demo Controller
/// Demonstrates different role-based access control scenarios
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/role-demo")]
[Tags("Role-Based Authorization")]
public class RoleBasedAuthController : ControllerBase
{
    private readonly ILogger<RoleBasedAuthController> _logger;

    public RoleBasedAuthController(ILogger<RoleBasedAuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Public endpoint - No authentication required
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult GetPublicInfo()
    {
        return Ok(new
        {
            Message = "This is a public endpoint accessible to everyone",
            Timestamp = DateTime.UtcNow,
            RequiredAuth = "None",
            AccessLevel = "Public"
        });
    }

    /// <summary>
    /// Any authenticated user can access this
    /// </summary>
    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult GetAuthenticatedInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("Authenticated access by user: {Username} with roles: {Roles}", 
            username, string.Join(", ", roles));

        return Ok(new
        {
            Message = "This endpoint requires authentication",
            Username = username,
            Roles = roles,
            RequiredAuth = "Any authenticated user",
            AccessLevel = "Authenticated"
        });
    }

    /// <summary>
    /// Admin-only endpoint
    /// </summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminOnlyInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("Admin access by user: {Username}", username);

        return Ok(new
        {
            Message = "This endpoint is for Admins only",
            Username = username,
            Roles = roles,
            RequiredAuth = "Admin role",
            AccessLevel = "Admin Only",
            SpecialData = new
            {
                SystemMetrics = "Server CPU: 45%, Memory: 67%",
                ActiveUsers = 1337,
                SystemHealth = "Optimal"
            }
        });
    }

    /// <summary>
    /// Manager or Admin endpoint
    /// </summary>
    [HttpGet("manager-or-admin")]
    [Authorize(Roles = "Admin,Manager")]
    public IActionResult GetManagerInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var department = User.FindFirst("department")?.Value;

        _logger.LogInformation("Manager/Admin access by user: {Username} from department: {Department}", 
            username, department);

        return Ok(new
        {
            Message = "This endpoint is for Managers and Admins",
            Username = username,
            Roles = roles,
            Department = department,
            RequiredAuth = "Manager or Admin role",
            AccessLevel = "Management",
            ManagementData = new
            {
                TeamPerformance = "Above average",
                QuarterlySales = "$1.2M",
                TeamSize = 15,
                RegionalMetrics = "North America: +12%, Europe: +8%"
            }
        });
    }

    /// <summary>
    /// Multiple role support - User, Manager, or Admin
    /// </summary>
    [HttpGet("user-or-above")]
    [Authorize(Policy = "UserOrAbove")]
    public IActionResult GetUserInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;

        return Ok(new
        {
            Message = "This endpoint is for Users, Managers, and Admins",
            Username = username,
            Roles = roles,
            SecurityLevel = securityLevel,
            RequiredAuth = "User, Manager, or Admin role",
            AccessLevel = "Standard Access",
            UserData = new
            {
                BookmarkedItems = 23,
                RecentActivity = "Viewed 5 books, Added 2 to wishlist",
                AccountStatus = "Active",
                Preferences = "Fiction, Mystery, Sci-Fi"
            }
        });
    }

    /// <summary>
    /// Support role endpoint
    /// </summary>
    [HttpGet("support-access")]
    [Authorize(Policy = "SupportAccess")]
    public IActionResult GetSupportInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var department = User.FindFirst("department")?.Value;

        return Ok(new
        {
            Message = "This endpoint is for Support staff and above",
            Username = username,
            Roles = roles,
            Department = department,
            RequiredAuth = "Support, Manager, or Admin role",
            AccessLevel = "Customer Support",
            SupportData = new
            {
                OpenTickets = 47,
                AvgResponseTime = "2.3 hours",
                CustomerSatisfaction = "94%",
                AvailableLanguages = User.FindFirst("languages")?.Value ?? "EN"
            }
        });
    }

    /// <summary>
    /// Role hierarchy demonstration
    /// </summary>
    [HttpGet("role-hierarchy")]
    [Authorize]
    public IActionResult GetRoleHierarchy()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;

        // Determine access level based on roles
        string accessLevel = "Unknown";
        List<string> availableActions = new();

        if (roles.Contains("Admin"))
        {
            accessLevel = "Full Administrative Access";
            availableActions = new List<string> 
            { 
                "Manage Users", "System Configuration", "View Reports", 
                "Manage Books", "Delete Records", "Access Logs" 
            };
        }
        else if (roles.Contains("Manager"))
        {
            accessLevel = "Management Access";
            availableActions = new List<string> 
            { 
                "View Reports", "Manage Team", "Manage Books", "View Users" 
            };
        }
        else if (roles.Contains("Support"))
        {
            accessLevel = "Customer Support Access";
            availableActions = new List<string> 
            { 
                "View Customer Data", "Handle Tickets", "View Orders", "Basic Reports" 
            };
        }
        else if (roles.Contains("User"))
        {
            accessLevel = "Standard User Access";
            availableActions = new List<string> 
            { 
                "Browse Books", "Manage Profile", "View Orders", "Write Reviews" 
            };
        }
        else if (roles.Contains("Guest"))
        {
            accessLevel = "Guest Access";
            availableActions = new List<string> { "Browse Books", "View Public Content" };
        }

        return Ok(new
        {
            Message = "Role hierarchy and permissions demonstration",
            Username = username,
            Roles = roles,
            Permissions = permissions,
            SecurityLevel = securityLevel,
            AccessLevel = accessLevel,
            AvailableActions = availableActions,
            RoleHierarchy = new
            {
                Level1 = "Guest (Read-only access to public content)",
                Level2 = "User (Standard user operations)",
                Level3 = "Support (Customer service operations)",
                Level4 = "Manager (Team and resource management)",
                Level5 = "Admin (Full system access)"
            }
        });
    }

    /// <summary>
    /// Multiple authorization attributes example
    /// </summary>
    [HttpGet("multiple-auth")]
    [Authorize(Roles = "Admin,Manager")]
    [Authorize(Policy = "HighSecurityLevel")]
    public IActionResult GetMultipleAuthInfo()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires BOTH Admin/Manager role AND high security level",
            Username = username,
            Roles = roles,
            SecurityLevel = securityLevel,
            RequiredAuth = "Multiple conditions: (Admin OR Manager) AND (Security Level ≥ 3)",
            AccessLevel = "High Security Management",
            SensitiveData = new
            {
                FinancialMetrics = "Revenue: $5.2M, Profit: $1.3M",
                SecurityIncidents = "0 this month",
                ComplianceStatus = "100% SOX, GDPR, PCI-DSS",
                ExecutiveDashboard = "Available"
            }
        });
    }
}