using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebCoreAPI.Authorization;

/// <summary>
/// Security Level Authorization Requirement
/// </summary>
public class SecurityLevelRequirement : IAuthorizationRequirement
{
    public int MinimumLevel { get; }

    public SecurityLevelRequirement(int minimumLevel)
    {
        MinimumLevel = minimumLevel;
    }
}

/// <summary>
/// Security Level Authorization Handler
/// </summary>
public class SecurityLevelHandler : AuthorizationHandler<SecurityLevelRequirement>
{
    private readonly ILogger<SecurityLevelHandler> _logger;

    public SecurityLevelHandler(ILogger<SecurityLevelHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SecurityLevelRequirement requirement)
    {
        var securityLevelClaim = context.User.FindFirst("security_level");
        
        if (securityLevelClaim != null && int.TryParse(securityLevelClaim.Value, out int userLevel))
        {
            if (userLevel >= requirement.MinimumLevel)
            {
                _logger.LogInformation("Access granted: User has security level {UserLevel}, required {RequiredLevel}", 
                    userLevel, requirement.MinimumLevel);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Access denied: User has security level {UserLevel}, required {RequiredLevel}", 
                    userLevel, requirement.MinimumLevel);
            }
        }
        else
        {
            _logger.LogWarning("Access denied: No security level claim found");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Department Authorization Requirement
/// </summary>
public class DepartmentRequirement : IAuthorizationRequirement
{
    public List<string> AllowedDepartments { get; }

    public DepartmentRequirement(params string[] allowedDepartments)
    {
        AllowedDepartments = allowedDepartments.ToList();
    }
}

/// <summary>
/// Department Authorization Handler
/// </summary>
public class DepartmentHandler : AuthorizationHandler<DepartmentRequirement>
{
    private readonly ILogger<DepartmentHandler> _logger;

    public DepartmentHandler(ILogger<DepartmentHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DepartmentRequirement requirement)
    {
        var departmentClaim = context.User.FindFirst("department");
        
        if (departmentClaim != null)
        {
            if (requirement.AllowedDepartments.Contains(departmentClaim.Value, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Access granted: User department {Department} is allowed", 
                    departmentClaim.Value);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Access denied: User department {Department} not in allowed list: {AllowedDepartments}", 
                    departmentClaim.Value, string.Join(", ", requirement.AllowedDepartments));
            }
        }
        else
        {
            _logger.LogWarning("Access denied: No department claim found");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Permission Authorization Requirement
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Permission Authorization Handler
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionHandler> _logger;

    public PermissionHandler(ILogger<PermissionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var permissionClaims = context.User.FindAll("permission");
        var userPermissions = permissionClaims.Select(c => c.Value).ToList();
        
        if (userPermissions.Contains(requirement.Permission))
        {
            _logger.LogInformation("Access granted: User has required permission {Permission}", 
                requirement.Permission);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Access denied: User lacks required permission {Permission}. User permissions: {UserPermissions}", 
                requirement.Permission, string.Join(", ", userPermissions));
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Region-based Authorization Requirement
/// </summary>
public class RegionRequirement : IAuthorizationRequirement
{
    public List<string> AllowedRegions { get; }

    public RegionRequirement(params string[] allowedRegions)
    {
        AllowedRegions = allowedRegions.ToList();
    }
}

/// <summary>
/// Region Authorization Handler
/// </summary>
public class RegionHandler : AuthorizationHandler<RegionRequirement>
{
    private readonly ILogger<RegionHandler> _logger;

    public RegionHandler(ILogger<RegionHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RegionRequirement requirement)
    {
        var regionClaim = context.User.FindFirst("region");
        
        if (regionClaim != null)
        {
            if (requirement.AllowedRegions.Contains(regionClaim.Value, StringComparer.OrdinalIgnoreCase) ||
                requirement.AllowedRegions.Contains("Global", StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Access granted: User region {Region} is allowed", 
                    regionClaim.Value);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Access denied: User region {Region} not in allowed list: {AllowedRegions}", 
                    regionClaim.Value, string.Join(", ", requirement.AllowedRegions));
            }
        }
        else
        {
            _logger.LogWarning("Access denied: No region claim found");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Composite Authorization Requirement (Multiple conditions)
/// </summary>
public class CompositeRequirement : IAuthorizationRequirement
{
    public int MinimumSecurityLevel { get; }
    public List<string> RequiredRoles { get; }
    public List<string> RequiredPermissions { get; }

    public CompositeRequirement(int minimumSecurityLevel, string[] requiredRoles, string[] requiredPermissions)
    {
        MinimumSecurityLevel = minimumSecurityLevel;
        RequiredRoles = requiredRoles.ToList();
        RequiredPermissions = requiredPermissions.ToList();
    }
}

/// <summary>
/// Composite Authorization Handler
/// </summary>
public class CompositeHandler : AuthorizationHandler<CompositeRequirement>
{
    private readonly ILogger<CompositeHandler> _logger;

    public CompositeHandler(ILogger<CompositeHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CompositeRequirement requirement)
    {
        var reasons = new List<string>();

        // Check security level
        var securityLevelClaim = context.User.FindFirst("security_level");
        if (securityLevelClaim == null || !int.TryParse(securityLevelClaim.Value, out int userLevel) || 
            userLevel < requirement.MinimumSecurityLevel)
        {
            reasons.Add($"Insufficient security level (required: {requirement.MinimumSecurityLevel})");
        }

        // Check roles
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var hasRequiredRole = requirement.RequiredRoles.Any(role => userRoles.Contains(role));
        if (!hasRequiredRole)
        {
            reasons.Add($"Missing required role (one of: {string.Join(", ", requirement.RequiredRoles)})");
        }

        // Check permissions
        var userPermissions = context.User.FindAll("permission").Select(c => c.Value).ToList();
        var missingPermissions = requirement.RequiredPermissions.Except(userPermissions).ToList();
        if (missingPermissions.Any())
        {
            reasons.Add($"Missing permissions: {string.Join(", ", missingPermissions)}");
        }

        if (reasons.Count == 0)
        {
            _logger.LogInformation("Composite authorization granted for user {Username}", 
                context.User.Identity?.Name);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Composite authorization denied for user {Username}. Reasons: {Reasons}", 
                context.User.Identity?.Name, string.Join("; ", reasons));
        }

        return Task.CompletedTask;
    }
}