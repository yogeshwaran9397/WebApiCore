using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Comprehensive Authorization Demo Controller
/// Demonstrates all authorization approaches in a unified controller with versioning
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth-demo")]
[Tags("Comprehensive Authorization Demo")]
public class AuthorizationDemoController : ControllerBase
{
    private readonly ILogger<AuthorizationDemoController> _logger;

    public AuthorizationDemoController(ILogger<AuthorizationDemoController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get authorization overview and available endpoints
    /// </summary>
    [HttpGet("overview")]
    [AllowAnonymous]
    public IActionResult GetAuthorizationOverview()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        
        return Ok(new
        {
            Message = "Comprehensive Authorization System Overview",
            Version = apiVersion?.ToString(),
            AuthorizationTypes = new
            {
                RoleBased = new
                {
                    Description = "Authorization based on user roles (Admin, Manager, User, Support, Guest)",
                    Examples = new[]
                    {
                        "[Authorize(Roles = \"Admin\")]",
                        "[Authorize(Roles = \"Admin,Manager\")]",
                        "[Authorize(Policy = \"UserOrAbove\")]"
                    }
                },
                PolicyBased = new
                {
                    Description = "Authorization using custom policies with requirements and handlers",
                    Examples = new[]
                    {
                        "[Authorize(Policy = \"SecurityLevel3\")]",
                        "[Authorize(Policy = \"ITOrSalesDepartment\")]",
                        "[Authorize(Policy = \"CanReadUsers\")]"
                    }
                },
                ClaimsBased = new
                {
                    Description = "Authorization based on JWT token claims and claim values",
                    Examples = new[]
                    {
                        "[Authorize(Policy = \"ITDepartment\")]",
                        "[Authorize(Policy = \"HighSecurityLevel\")]",
                        "[Authorize(Policy = \"ExecutiveAccess\")]"
                    }
                }
            },
            AvailableEndpoints = new
            {
                Public = "/auth-demo/overview (This endpoint)",
                Authentication = "/auth-demo/token-info (Any authenticated user)",
                RoleBased = "/auth-demo/role-matrix (Different role requirements)",
                PolicyBased = "/auth-demo/policy-matrix (Custom policy examples)",
                ClaimsBased = "/auth-demo/claims-matrix (Claim-based access)",
                Comprehensive = "/auth-demo/comprehensive-test (All authorization types)"
            },
            TestUsers = new[]
            {
                new { Username = "admin", Roles = new[] { "Admin", "Manager", "User" }, SecurityLevel = 5, Department = "IT" },
                new { Username = "manager", Roles = new[] { "Manager", "User" }, SecurityLevel = 3, Department = "Sales" },
                new { Username = "user", Roles = new[] { "User" }, SecurityLevel = 1, Department = "Sales" },
                new { Username = "support", Roles = new[] { "Support", "User" }, SecurityLevel = 2, Department = "Customer Service" },
                new { Username = "guest", Roles = new[] { "Guest" }, SecurityLevel = 0, Department = "None" }
            }
        });
    }

    /// <summary>
    /// Get JWT token information for authenticated users
    /// </summary>
    [HttpGet("token-info")]
    [Authorize]
    public IActionResult GetTokenInfo()
    {
        var username = User.Identity?.Name;
        var userId = User.FindFirst("user_id")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return Ok(new
        {
            Message = "JWT Token Information",
            UserInfo = new
            {
                UserId = userId,
                Username = username,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                FullName = User.FindFirst("full_name")?.Value
            },
            Authorization = new
            {
                Roles = roles,
                Permissions = permissions,
                SecurityLevel = User.FindFirst("security_level")?.Value,
                Department = User.FindFirst("department")?.Value,
                Region = User.FindFirst("region")?.Value
            },
            TokenClaims = new
            {
                StandardClaims = allClaims.Where(c => c.Type.StartsWith("http://schemas")).ToList(),
                CustomClaims = allClaims.Where(c => !c.Type.StartsWith("http://schemas")).ToList(),
                TotalClaimsCount = allClaims.Count
            }
        });
    }

    /// <summary>
    /// Role-based authorization matrix demonstration
    /// </summary>
    [HttpGet("role-matrix")]
    [Authorize]
    public IActionResult GetRoleMatrix()
    {
        var username = User.Identity?.Name;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Test access to different role requirements
        var roleTests = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            HasAdminRole = userRoles.Contains("Admin"),
            HasManagerRole = userRoles.Contains("Manager"),
            HasUserRole = userRoles.Contains("User"),
            HasSupportRole = userRoles.Contains("Support"),
            HasGuestRole = userRoles.Contains("Guest"),
            
            // Policy-based role checks
            CanAccessAdminOnly = userRoles.Contains("Admin"),
            CanAccessManagerOrAdmin = userRoles.Contains("Admin") || userRoles.Contains("Manager"),
            CanAccessUserOrAbove = userRoles.Contains("Admin") || userRoles.Contains("Manager") || userRoles.Contains("User"),
            CanAccessSupport = userRoles.Contains("Admin") || userRoles.Contains("Manager") || userRoles.Contains("Support")
        };

        return Ok(new
        {
            Message = "Role-based Authorization Matrix",
            Username = username,
            UserRoles = userRoles,
            RoleTests = roleTests,
            RoleHierarchy = new
            {
                Level5_Admin = "Full system access - Can access all endpoints",
                Level4_Manager = "Management access - Can access team and resource management", 
                Level3_User = "Standard access - Can access general user features",
                Level2_Support = "Customer service access - Can access support tools",
                Level1_Guest = "Limited access - Can only access public content"
            },
            AccessMatrix = new[]
            {
                new { Endpoint = "Public Info", Guest = "✓", User = "✓", Support = "✓", Manager = "✓", Admin = "✓" },
                new { Endpoint = "User Profile", Guest = "✗", User = "✓", Support = "✓", Manager = "✓", Admin = "✓" },
                new { Endpoint = "Support Tools", Guest = "✗", User = "✗", Support = "✓", Manager = "✓", Admin = "✓" },
                new { Endpoint = "Management Reports", Guest = "✗", User = "✗", Support = "✗", Manager = "✓", Admin = "✓" },
                new { Endpoint = "Admin Panel", Guest = "✗", User = "✗", Support = "✗", Manager = "✗", Admin = "✓" }
            }
        });
    }

    /// <summary>
    /// Policy-based authorization matrix demonstration
    /// </summary>
    [HttpGet("policy-matrix")]
    [Authorize]
    public IActionResult GetPolicyMatrix()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var department = User.FindFirst("department")?.Value;
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        var region = User.FindFirst("region")?.Value;

        // Test various policy requirements
        int.TryParse(securityLevel, out int userSecLevel);

        var policyTests = new
        {
            SecurityLevel = new
            {
                Level2OrHigher = userSecLevel >= 2,
                Level3OrHigher = userSecLevel >= 3,
                Level4OrHigher = userSecLevel >= 4,
                Level5Required = userSecLevel >= 5
            },
            Department = new
            {
                IsIT = department == "IT",
                IsSales = department == "Sales",
                IsCustomerService = department == "Customer Service",
                IsITOrSales = department == "IT" || department == "Sales"
            },
            Permissions = new
            {
                CanReadUsers = permissions.Contains("users.read"),
                CanWriteUsers = permissions.Contains("users.write"),
                CanDeleteUsers = permissions.Contains("users.delete"),
                CanManageBooks = permissions.Contains("books.write"),
                HasSystemAdmin = permissions.Contains("system.admin")
            },
            Region = new
            {
                IsNorthAmerica = region == "North America" || region == "Global",
                IsEurope = region == "Europe" || region == "Global",
                IsGlobal = region == "Global"
            }
        };

        return Ok(new
        {
            Message = "Policy-based Authorization Matrix",
            Username = username,
            UserAttributes = new
            {
                SecurityLevel = securityLevel,
                Department = department,
                Region = region,
                PermissionsCount = permissions.Count
            },
            PolicyTests = policyTests,
            PolicyDefinitions = new
            {
                SecurityPolicies = "Based on minimum security clearance level (1-5)",
                DepartmentPolicies = "Based on user's department affiliation",
                PermissionPolicies = "Based on specific granted permissions",
                RegionPolicies = "Based on user's regional assignment or global access",
                CompositePolicies = "Combination of multiple requirements (role + security + permissions)"
            }
        });
    }

    /// <summary>
    /// Claims-based authorization matrix demonstration  
    /// </summary>
    [HttpGet("claims-matrix")]
    [Authorize]
    public IActionResult GetClaimsMatrix()
    {
        var username = User.Identity?.Name;
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

        // Extract specific claims for testing
        var salaryBand = claims.GetValueOrDefault("salary_band");
        var employeeId = claims.GetValueOrDefault("employee_id");
        var hireDate = claims.GetValueOrDefault("hire_date");
        var clearanceLevel = claims.GetValueOrDefault("clearance_level");

        // Perform claims-based tests
        var claimsTests = new
        {
            SalaryBandTests = new
            {
                IsExecutive = salaryBand == "Executive",
                IsManager = salaryBand == "Manager", 
                IsExecutiveOrManager = salaryBand == "Executive" || salaryBand == "Manager"
            },
            TenureTests = new
            {
                HasHireDate = !string.IsNullOrEmpty(hireDate),
                IsVeteranEmployee = DateTime.TryParse(hireDate, out DateTime hire) && (DateTime.UtcNow - hire).TotalDays > 365
            },
            ClearanceTests = new
            {
                HasTopSecretClearance = clearanceLevel == "TOP_SECRET",
                HasSecretClearance = clearanceLevel == "SECRET" || clearanceLevel == "TOP_SECRET"
            },
            IdentityTests = new
            {
                HasEmployeeId = !string.IsNullOrEmpty(employeeId),
                EmployeeIdFormat = employeeId?.StartsWith("EMP") ?? false
            }
        };

        return Ok(new
        {
            Message = "Claims-based Authorization Matrix",
            Username = username,
            ClaimsAnalysis = new
            {
                StandardClaims = claims.Where(kvp => kvp.Key.StartsWith("http://schemas")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                CustomClaims = claims.Where(kvp => !kvp.Key.StartsWith("http://schemas")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                TotalClaims = claims.Count
            },
            ClaimsTests = claimsTests,
            ClaimsBasedPolicies = new
            {
                ITDepartment = "Requires 'department' claim = 'IT'",
                HighSecurityLevel = "Requires 'security_level' claim ∈ {3, 4, 5}",
                ExecutiveAccess = "Requires 'salary_band' claim ∈ {'Executive', 'Manager'}",
                RegionalAccess = "Based on 'region' claim value",
                CustomValidation = "Complex logic combining multiple claim values"
            }
        });
    }

    /// <summary>
    /// Comprehensive authorization test combining all approaches
    /// Version 1.0: Basic comprehensive test
    /// Version 2.0: Enhanced with additional metrics and analysis
    /// </summary>
    [HttpGet("comprehensive-test")]
    [Authorize]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public IActionResult GetComprehensiveTest()
    {
        var apiVersion = HttpContext.GetRequestedApiVersion();
        var username = User.Identity?.Name;
        var userId = User.FindFirst("user_id")?.Value;

        // Gather all authorization data
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;
        var department = User.FindFirst("department")?.Value;
        var region = User.FindFirst("region")?.Value;
        var salaryBand = User.FindFirst("salary_band")?.Value;

        // Calculate authorization score
        var authScore = CalculateAuthorizationScore(roles, permissions, securityLevel, department, salaryBand);

        var response = new
        {
            Message = "Comprehensive Authorization Analysis",
            Version = apiVersion?.ToString(),
            UserIdentity = new
            {
                UserId = userId,
                Username = username,
                FullName = User.FindFirst("full_name")?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value
            },
            AuthorizationProfile = new
            {
                Roles = roles,
                Permissions = permissions,
                SecurityLevel = securityLevel,
                Department = department,
                Region = region,
                SalaryBand = salaryBand,
                AuthorizationScore = authScore.Score,
                AccessLevel = authScore.Level
            },
            RoleBasedAccess = new
            {
                CanAccessPublic = true,
                CanAccessUser = roles.Contains("User") || roles.Contains("Manager") || roles.Contains("Admin"),
                CanAccessSupport = roles.Contains("Support") || roles.Contains("Manager") || roles.Contains("Admin"),
                CanAccessManager = roles.Contains("Manager") || roles.Contains("Admin"),
                CanAccessAdmin = roles.Contains("Admin")
            },
            PolicyBasedAccess = GetPolicyBasedAccess(securityLevel, department, permissions, region),
            ClaimsBasedAccess = GetClaimsBasedAccess(salaryBand, department, securityLevel),
            RecommendedActions = GetRecommendedActions(authScore.Score, roles, permissions)
        };

        // Version 2.0 enhancements
        if (apiVersion?.MajorVersion >= 2)
        {
            var enhancedResponse = new
            {
                response.Message,
                response.Version,
                response.UserIdentity,
                response.AuthorizationProfile,
                response.RoleBasedAccess,
                response.PolicyBasedAccess,
                response.ClaimsBasedAccess,
                response.RecommendedActions,
                
                // V2.0 Enhancements
                SecurityAnalysis = GetSecurityAnalysis(roles, securityLevel, permissions),
                ComplianceStatus = GetComplianceStatus(department, securityLevel, salaryBand),
                AccessPatterns = GetAccessPatterns(roles, permissions, department),
                RiskAssessment = GetRiskAssessment(authScore.Score, roles, permissions),
                AuditTrail = new
                {
                    LastAccess = DateTime.UtcNow,
                    AccessMethod = "JWT Bearer Token",
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                }
            };

            return Ok(enhancedResponse);
        }

        return Ok(response);
    }

    #region Helper Methods

    private (int Score, string Level) CalculateAuthorizationScore(List<string> roles, List<string> permissions, string? securityLevel, string? department, string? salaryBand)
    {
        int score = 0;

        // Role scoring
        if (roles.Contains("Admin")) score += 50;
        else if (roles.Contains("Manager")) score += 40;
        else if (roles.Contains("User")) score += 25;
        else if (roles.Contains("Support")) score += 20;
        else if (roles.Contains("Guest")) score += 10;

        // Security level scoring
        if (int.TryParse(securityLevel, out int level))
        {
            score += level * 5;
        }

        // Permission scoring
        score += permissions.Count * 2;

        // Department scoring
        if (department == "IT") score += 15;
        else if (department == "Sales") score += 10;
        else if (department == "Customer Service") score += 8;

        // Salary band scoring
        if (salaryBand == "Executive") score += 25;
        else if (salaryBand == "Manager") score += 15;

        string accessLevel = score switch
        {
            >= 90 => "Maximum Access",
            >= 70 => "High Access",
            >= 50 => "Medium Access",
            >= 30 => "Basic Access",
            _ => "Limited Access"
        };

        return (score, accessLevel);
    }

    private object GetPolicyBasedAccess(string? securityLevel, string? department, List<string> permissions, string? region)
    {
        int.TryParse(securityLevel, out int level);

        return new
        {
            SecurityPolicies = new
            {
                Level2Access = level >= 2,
                Level3Access = level >= 3,
                Level4Access = level >= 4,
                Level5Access = level >= 5
            },
            DepartmentPolicies = new
            {
                ITAccess = department == "IT",
                SalesAccess = department == "Sales",
                ITOrSalesAccess = department == "IT" || department == "Sales",
                CustomerServiceAccess = department == "Customer Service"
            },
            PermissionPolicies = new
            {
                CanReadUsers = permissions.Contains("users.read"),
                CanWriteUsers = permissions.Contains("users.write"),
                CanDeleteUsers = permissions.Contains("users.delete"),
                CanManageBooks = permissions.Contains("books.write")
            },
            RegionPolicies = new
            {
                NorthAmericaAccess = region == "North America" || region == "Global",
                EuropeAccess = region == "Europe" || region == "Global",
                GlobalAccess = region == "Global"
            }
        };
    }

    private object GetClaimsBasedAccess(string? salaryBand, string? department, string? securityLevel)
    {
        return new
        {
            ExecutiveAccess = salaryBand == "Executive" || salaryBand == "Manager",
            DepartmentClaimAccess = !string.IsNullOrEmpty(department),
            HighSecurityClaim = int.TryParse(securityLevel, out int level) && level >= 3,
            SeniorStaffAccess = (salaryBand == "Executive" || salaryBand == "Manager") && level >= 2
        };
    }

    private string[] GetRecommendedActions(int authScore, List<string> roles, List<string> permissions)
    {
        var actions = new List<string>();

        if (authScore < 30)
        {
            actions.Add("Consider requesting additional permissions for enhanced access");
        }

        if (!roles.Any())
        {
            actions.Add("No roles assigned - contact administrator for role assignment");
        }

        if (permissions.Count < 3)
        {
            actions.Add("Limited permissions detected - may need additional access rights");
        }

        if (authScore >= 80)
        {
            actions.Add("High-level access detected - ensure compliance with security policies");
        }

        return actions.ToArray();
    }

    private object GetSecurityAnalysis(List<string> roles, string? securityLevel, List<string> permissions)
    {
        return new
        {
            RiskLevel = roles.Contains("Admin") ? "High" : roles.Contains("Manager") ? "Medium" : "Low",
            RequiredMFA = int.TryParse(securityLevel, out int level) && level >= 3,
            PermissionScope = permissions.Count switch
            {
                >= 8 => "Broad",
                >= 5 => "Moderate", 
                >= 2 => "Limited",
                _ => "Minimal"
            },
            ComplianceRequired = roles.Contains("Admin") || (level >= 4)
        };
    }

    private object GetComplianceStatus(string? department, string? securityLevel, string? salaryBand)
    {
        return new
        {
            SOXCompliance = salaryBand == "Executive" ? "Required" : "Not Required",
            GDPRCompliance = department == "IT" || department == "Customer Service" ? "Required" : "Standard",
            SecurityTraining = int.TryParse(securityLevel, out int level) && level >= 3 ? "Annual" : "Bi-Annual",
            BackgroundCheck = level >= 4 ? "Required Every 2 Years" : "Initial Only"
        };
    }

    private object GetAccessPatterns(List<string> roles, List<string> permissions, string? department)
    {
        return new
        {
            PrimaryRole = roles.FirstOrDefault() ?? "None",
            AccessFrequency = permissions.Count >= 5 ? "High" : permissions.Count >= 3 ? "Medium" : "Low",
            DepartmentalAccess = department != null ? $"Primary access to {department} resources" : "No departmental access",
            CrossFunctional = roles.Count > 1 ? "Multi-role access enabled" : "Single role access"
        };
    }

    private object GetRiskAssessment(int authScore, List<string> roles, List<string> permissions)
    {
        string riskLevel = (authScore, roles.Contains("Admin")) switch
        {
            (>= 80, true) => "Critical - Admin with high privileges",
            (>= 70, _) => "High - Elevated access levels",
            (>= 50, _) => "Medium - Standard access",
            (>= 30, _) => "Low - Basic access",
            _ => "Minimal - Limited access"
        };

        return new
        {
            OverallRisk = riskLevel,
            AdminPrivileges = roles.Contains("Admin"),
            HighPermissionCount = permissions.Count >= 6,
            RecommendedMonitoring = authScore >= 70 ? "Enhanced" : "Standard"
        };
    }

    #endregion
}