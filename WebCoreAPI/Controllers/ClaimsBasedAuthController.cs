using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Claims-Based Authorization Demo Controller
/// Demonstrates authorization based on custom claims and claim values
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/claims-demo")]
[Tags("Claims-Based Authorization")]
public class ClaimsBasedAuthController : ControllerBase
{
    private readonly ILogger<ClaimsBasedAuthController> _logger;

    public ClaimsBasedAuthController(ILogger<ClaimsBasedAuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all user claims (Authenticated users only)
    /// </summary>
    [HttpGet("my-claims")]
    [Authorize]
    public IActionResult GetMyClaims()
    {
        var claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
        var username = User.Identity?.Name;

        return Ok(new
        {
            Message = "All claims for the authenticated user",
            Username = username,
            ClaimsCount = claims.Count,
            Claims = claims.OrderBy(c => c.Type),
            StandardClaims = new
            {
                NameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            CustomClaims = new
            {
                Department = User.FindFirst("department")?.Value,
                SecurityLevel = User.FindFirst("security_level")?.Value,
                Region = User.FindFirst("region")?.Value,
                EmployeeId = User.FindFirst("employee_id")?.Value,
                HireDate = User.FindFirst("hire_date")?.Value
            }
        });
    }

    /// <summary>
    /// IT Department claim-based access
    /// </summary>
    [HttpGet("it-department-only")]
    [Authorize(Policy = "ITDepartment")]
    public IActionResult GetITDepartmentInfo()
    {
        var username = User.Identity?.Name;
        var department = User.FindFirst("department")?.Value;
        var employeeId = User.FindFirst("employee_id")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires 'department' claim with value 'IT'",
            Username = username,
            Department = department,
            EmployeeId = employeeId,
            RequiredClaim = "department = 'IT'",
            AccessLevel = "IT Department Only",
            ITResources = new
            {
                DevTools = new[] { "Visual Studio", "Docker", "Kubernetes", "Azure DevOps" },
                ServerAccess = new[] { "Production servers", "Staging environment", "Development sandbox" },
                Permissions = new[] { "Deploy code", "Manage databases", "Configure networks" },
                OnCallSchedule = "Current week: Alice (Primary), Bob (Secondary)",
                ITMetrics = new
                {
                    SystemUptime = "99.97%",
                    IncidentResponse = "< 15 minutes average",
                    SecurityCompliance = "100%"
                }
            }
        });
    }

    /// <summary>
    /// High security level claim-based access
    /// </summary>
    [HttpGet("high-security")]
    [Authorize(Policy = "HighSecurityLevel")]
    public IActionResult GetHighSecurityInfo()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var clearanceLevel = User.FindFirst("clearance_level")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires high security level claims (3, 4, or 5)",
            Username = username,
            SecurityLevel = securityLevel,
            ClearanceLevel = clearanceLevel,
            RequiredClaim = "security_level ∈ {3, 4, 5}",
            AccessLevel = "High Security Clearance",
            SecurityData = new
            {
                Classification = GetClassificationLevel(securityLevel),
                AccessibleSystems = GetAccessibleSystems(securityLevel),
                SecurityProtocols = new[]
                {
                    "Two-factor authentication required",
                    "Biometric verification for Level 4+",
                    "Background check every 2 years",
                    "Security training annually"
                },
                IncidentReporting = "Direct to security team within 1 hour"
            }
        });
    }

    /// <summary>
    /// Executive access based on salary band claim
    /// </summary>
    [HttpGet("executive-access")]
    [Authorize(Policy = "ExecutiveAccess")]
    public IActionResult GetExecutiveInfo()
    {
        var username = User.Identity?.Name;
        var salaryBand = User.FindFirst("salary_band")?.Value;
        var hireDate = User.FindFirst("hire_date")?.Value;
        var department = User.FindFirst("department")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires executive-level salary band claims",
            Username = username,
            SalaryBand = salaryBand,
            HireDate = hireDate,
            Department = department,
            RequiredClaim = "salary_band ∈ {'Executive', 'Manager'}",
            AccessLevel = "Executive Level",
            ExecutiveData = new
            {
                BoardMeetings = new[]
                {
                    "Q4 2025 Strategic Planning - Dec 15",
                    "Budget Review 2026 - Jan 10",
                    "Market Analysis - Jan 25"
                },
                ExecutiveReports = new[]
                {
                    "Company Performance Dashboard",
                    "Competitive Analysis Report", 
                    "Risk Assessment Summary",
                    "Shareholder Communications"
                },
                StrategicInitiatives = new
                {
                    DigitalTransformation = "75% complete",
                    MarketExpansion = "Phase 2 planning",
                    TalentAcquisition = "25 senior hires YTD",
                    Innovation = "3 patents filed this year"
                }
            }
        });
    }

    /// <summary>
    /// Employee ID based access (specific employee claim)
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize]
    public IActionResult GetEmployeeSpecificInfo(string employeeId)
    {
        var username = User.Identity?.Name;
        var userEmployeeId = User.FindFirst("employee_id")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Check if user is accessing their own data or has admin rights
        bool canAccess = userEmployeeId == employeeId || roles.Contains("Admin") || roles.Contains("Manager");

        if (!canAccess)
        {
            return Forbid();
        }

        return Ok(new
        {
            Message = "Employee-specific information access",
            Username = username,
            YourEmployeeId = userEmployeeId,
            RequestedEmployeeId = employeeId,
            AccessReason = userEmployeeId == employeeId ? "Own data access" : "Administrative access",
            RequiredClaim = $"employee_id = '{employeeId}' OR administrative role",
            EmployeeData = new
            {
                EmployeeId = employeeId,
                PersonalInfo = userEmployeeId == employeeId ? GetPersonalInfo() : "Redacted for privacy",
                WorkHistory = GetWorkHistory(employeeId),
                Performance = roles.Contains("Admin") || roles.Contains("Manager") ? GetPerformanceMetrics() : "Restricted",
                Benefits = userEmployeeId == employeeId ? GetBenefitsInfo() : "Not accessible"
            }
        });
    }

    /// <summary>
    /// Regional data access based on region claim
    /// </summary>
    [HttpGet("regional-data")]
    [Authorize]
    public IActionResult GetRegionalData()
    {
        var username = User.Identity?.Name;
        var userRegion = User.FindFirst("region")?.Value;
        var department = User.FindFirst("department")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Determine accessible regions
        var accessibleRegions = new List<string>();
        if (userRegion == "Global" || roles.Contains("Admin"))
        {
            accessibleRegions = new List<string> { "North America", "Europe", "Asia-Pacific", "South America" };
        }
        else if (!string.IsNullOrEmpty(userRegion))
        {
            accessibleRegions.Add(userRegion);
        }

        return Ok(new
        {
            Message = "Regional data access based on user's region claim",
            Username = username,
            UserRegion = userRegion,
            Department = department,
            AccessibleRegions = accessibleRegions,
            RequiredClaim = "region claim determines accessible regional data",
            RegionalData = accessibleRegions.ToDictionary(
                region => region,
                region => GetRegionalMetrics(region)
            ),
            AccessLevel = userRegion == "Global" ? "Global Access" : $"Regional Access: {userRegion}"
        });
    }

    /// <summary>
    /// Custom claim validation example
    /// </summary>
    [HttpGet("custom-validation")]
    [Authorize]
    public IActionResult GetCustomValidationInfo()
    {
        var username = User.Identity?.Name;
        var hireDate = User.FindFirst("hire_date")?.Value;
        var accountType = User.FindFirst("account_type")?.Value;
        
        // Custom validation: Check if user has been employed for more than 1 year
        bool isVeteranEmployee = false;
        if (DateTime.TryParse(hireDate, out DateTime hire))
        {
            isVeteranEmployee = (DateTime.UtcNow - hire).TotalDays > 365;
        }

        // Custom validation: Check account type
        bool isPremiumAccount = accountType != "Guest";

        var validationResults = new
        {
            IsVeteranEmployee = isVeteranEmployee,
            IsPremiumAccount = isPremiumAccount,
            HasFullAccess = isVeteranEmployee && isPremiumAccount
        };

        return Ok(new
        {
            Message = "Custom claim validation demonstration",
            Username = username,
            HireDate = hireDate,
            AccountType = accountType,
            ValidationResults = validationResults,
            CustomRules = new
            {
                VeteranEmployeeRule = "hire_date > 365 days ago",
                PremiumAccountRule = "account_type != 'Guest'",
                FullAccessRule = "VeteranEmployee AND PremiumAccount"
            },
            AccessibleFeatures = GetAccessibleFeatures(validationResults.HasFullAccess, validationResults.IsVeteranEmployee, validationResults.IsPremiumAccount)
        });
    }

    /// <summary>
    /// Composite claims example (multiple claim requirements)
    /// </summary>
    [HttpGet("composite-claims")]
    [Authorize]
    public IActionResult GetCompositeClaimsInfo()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var department = User.FindFirst("department")?.Value;
        var salaryBand = User.FindFirst("salary_band")?.Value;
        var region = User.FindFirst("region")?.Value;

        // Complex claim-based logic
        bool hasHighSecurity = int.TryParse(securityLevel, out int level) && level >= 3;
        bool isManagementDept = department == "IT" || department == "Sales";
        bool isSeniorLevel = salaryBand == "Executive" || salaryBand == "Manager";
        bool hasGlobalAccess = region == "Global";

        var accessScore = 0;
        if (hasHighSecurity) accessScore += 25;
        if (isManagementDept) accessScore += 25;
        if (isSeniorLevel) accessScore += 30;
        if (hasGlobalAccess) accessScore += 20;

        string accessLevel = accessScore switch
        {
            >= 80 => "Executive Access",
            >= 60 => "Senior Management Access", 
            >= 40 => "Management Access",
            >= 20 => "Department Access",
            _ => "Basic Access"
        };

        return Ok(new
        {
            Message = "Composite claims authorization demonstration",
            Username = username,
            ClaimValues = new
            {
                SecurityLevel = securityLevel,
                Department = department,
                SalaryBand = salaryBand,
                Region = region
            },
            ClaimEvaluations = new
            {
                HasHighSecurity = hasHighSecurity,
                IsManagementDept = isManagementDept,
                IsSeniorLevel = isSeniorLevel,
                HasGlobalAccess = hasGlobalAccess
            },
            AccessScore = accessScore,
            AccessLevel = accessLevel,
            AvailableResources = GetResourcesByAccessLevel(accessLevel),
            ClaimRequirements = new
            {
                HighSecurity = "security_level >= 3 (+25 points)",
                ManagementDept = "department ∈ {'IT', 'Sales'} (+25 points)",
                SeniorLevel = "salary_band ∈ {'Executive', 'Manager'} (+30 points)",
                GlobalAccess = "region = 'Global' (+20 points)",
                Scoring = "Total points determine access level"
            }
        });
    }

    #region Helper Methods

    private string GetClassificationLevel(string? securityLevel)
    {
        return securityLevel switch
        {
            "3" => "Confidential",
            "4" => "Secret", 
            "5" => "Top Secret",
            _ => "Unclassified"
        };
    }

    private string[] GetAccessibleSystems(string? securityLevel)
    {
        return securityLevel switch
        {
            "3" => new[] { "Internal systems", "Customer database", "Financial reports" },
            "4" => new[] { "Internal systems", "Customer database", "Financial reports", "Strategic planning", "Legal documents" },
            "5" => new[] { "All systems", "Executive communications", "Board materials", "Merger & acquisition data" },
            _ => new[] { "Public systems only" }
        };
    }

    private object GetPersonalInfo()
    {
        return new
        {
            Address = "123 Main St, Anytown, USA",
            Phone = "(555) 123-4567",
            EmergencyContact = "Jane Doe - (555) 987-6543",
            BankingInfo = "Account ending in 4567"
        };
    }

    private object GetWorkHistory(string employeeId)
    {
        return new
        {
            StartDate = "2021-03-15",
            Positions = new[]
            {
                new { Title = "Software Developer", Department = "IT", Duration = "2021-2023" },
                new { Title = "Senior Developer", Department = "IT", Duration = "2023-Present" }
            },
            Promotions = 1,
            Awards = new[] { "Employee of the Month - June 2023", "Innovation Award 2024" }
        };
    }

    private object GetPerformanceMetrics()
    {
        return new
        {
            OverallRating = "Exceeds Expectations",
            LastReviewDate = "2024-06-15",
            Goals = new[] { "Lead team project", "Mentor junior developers", "Complete certification" },
            Achievements = "Delivered 3 major projects on time and under budget"
        };
    }

    private object GetBenefitsInfo()
    {
        return new
        {
            HealthInsurance = "Premium Plan - Family Coverage",
            RetirementPlan = "401k with 6% company match",
            VacationDays = "23 days available",
            StockOptions = "1,500 shares vested"
        };
    }

    private object GetRegionalMetrics(string region)
    {
        return region switch
        {
            "North America" => new { Revenue = "$5.2M", Growth = "+12%", Markets = 3 },
            "Europe" => new { Revenue = "$3.8M", Growth = "+8%", Markets = 7 },
            "Asia-Pacific" => new { Revenue = "$2.1M", Growth = "+25%", Markets = 5 },
            "South America" => new { Revenue = "$1.3M", Growth = "+18%", Markets = 4 },
            _ => new { Revenue = "N/A", Growth = "N/A", Markets = 0 }
        };
    }

    private object GetAccessibleFeatures(bool hasFullAccess, bool isVeteran, bool isPremium)
    {
        var features = new List<string> { "Basic catalog access" };
        
        if (isPremium) features.AddRange(new[] { "Advanced search", "Personalized recommendations" });
        if (isVeteran) features.AddRange(new[] { "Veteran employee discounts", "Early access to new features" });
        if (hasFullAccess) features.AddRange(new[] { "Premium content", "Executive reports", "Advanced analytics" });

        return features;
    }

    private object GetResourcesByAccessLevel(string accessLevel)
    {
        return accessLevel switch
        {
            "Executive Access" => new[] { "Board materials", "Strategic plans", "Executive dashboard", "All financial data" },
            "Senior Management Access" => new[] { "Management reports", "Department budgets", "Performance analytics", "Team metrics" },
            "Management Access" => new[] { "Team reports", "Project data", "Resource planning", "Basic analytics" },
            "Department Access" => new[] { "Department resources", "Team calendar", "Basic reports" },
            _ => new[] { "Public resources", "Basic information" }
        };
    }

    #endregion
}