using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebCoreAPI.Controllers;

/// <summary>
/// Policy-Based Authorization Demo Controller
/// Demonstrates custom authorization policies and requirements
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/policy-demo")]
[Tags("Policy-Based Authorization")]
public class PolicyBasedAuthController : ControllerBase
{
    private readonly ILogger<PolicyBasedAuthController> _logger;

    public PolicyBasedAuthController(ILogger<PolicyBasedAuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Security Level Policy Example
    /// </summary>
    [HttpGet("security-level-2")]
    [Authorize(Policy = "SecurityLevel2")]
    public IActionResult GetSecurityLevel2Info()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var department = User.FindFirst("department")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires Security Level 2 or higher",
            Username = username,
            SecurityLevel = securityLevel,
            Department = department,
            RequiredPolicy = "SecurityLevel2 (Minimum security clearance: 2)",
            AccessLevel = "Confidential",
            Data = new
            {
                Classification = "Confidential",
                Information = "Internal company metrics and basic financial data",
                MonthlyRevenue = "$425,000",
                EmployeeCount = 156
            }
        });
    }

    /// <summary>
    /// High Security Level Policy Example
    /// </summary>
    [HttpGet("security-level-4")]
    [Authorize(Policy = "SecurityLevel4")]
    public IActionResult GetSecurityLevel4Info()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var clearanceLevel = User.FindFirst("clearance_level")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires Security Level 4 or higher",
            Username = username,
            SecurityLevel = securityLevel,
            ClearanceLevel = clearanceLevel,
            RequiredPolicy = "SecurityLevel4 (Minimum security clearance: 4)",
            AccessLevel = "Secret",
            SensitiveData = new
            {
                Classification = "Secret",
                Information = "Strategic business plans and sensitive financial data",
                AnnualProjections = "$8.2M revenue target",
                MergerPlans = "Confidential acquisition discussions ongoing",
                CompetitorAnalysis = "Market intelligence reports available"
            }
        });
    }

    /// <summary>
    /// Top Secret Level Policy Example
    /// </summary>
    [HttpGet("security-level-5")]
    [Authorize(Policy = "SecurityLevel5")]
    public IActionResult GetSecurityLevel5Info()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var clearanceLevel = User.FindFirst("clearance_level")?.Value;

        return Ok(new
        {
            Message = "This endpoint requires Security Level 5 (Top Secret)",
            Username = username,
            SecurityLevel = securityLevel,
            ClearanceLevel = clearanceLevel,
            RequiredPolicy = "SecurityLevel5 (Maximum security clearance required)",
            AccessLevel = "Top Secret",
            TopSecretData = new
            {
                Classification = "Top Secret",
                Information = "Highly sensitive corporate strategy and executive decisions",
                BoardDecisions = "Classified board meeting minutes",
                LegalMatters = "Ongoing litigation strategy",
                IntellectualProperty = "Proprietary algorithms and trade secrets",
                ExecutiveCompensation = "C-level salary and bonus information"
            }
        });
    }

    /// <summary>
    /// Department-Based Policy Example
    /// </summary>
    [HttpGet("it-department")]
    [Authorize(Policy = "ITOrSalesDepartment")]
    public IActionResult GetITDepartmentInfo()
    {
        var username = User.Identity?.Name;
        var department = User.FindFirst("department")?.Value;
        var employeeId = User.FindFirst("employee_id")?.Value;

        return Ok(new
        {
            Message = "This endpoint is accessible to IT and Sales departments only",
            Username = username,
            Department = department,
            EmployeeId = employeeId,
            RequiredPolicy = "ITOrSalesDepartment (Department must be IT or Sales)",
            AccessLevel = "Department-Specific",
            DepartmentData = new
            {
                ITTools = new[] { "Azure DevOps", "Jira", "GitHub", "Docker" },
                SalesTools = new[] { "Salesforce", "HubSpot", "PowerBI", "Tableau" },
                SharedResources = "Internal knowledge base, training materials",
                CrossDepartmentProjects = "Digital transformation initiative"
            }
        });
    }

    /// <summary>
    /// Permission-Based Policy Example
    /// </summary>
    [HttpGet("user-management")]
    [Authorize(Policy = "CanReadUsers")]
    public IActionResult GetUserManagementInfo()
    {
        var username = User.Identity?.Name;
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        var canWrite = permissions.Contains("users.write");
        var canDelete = permissions.Contains("users.delete");

        return Ok(new
        {
            Message = "This endpoint requires 'users.read' permission",
            Username = username,
            Permissions = permissions,
            RequiredPolicy = "CanReadUsers (Must have 'users.read' permission)",
            AccessLevel = "Permission-Based",
            UserManagementData = new
            {
                TotalUsers = 156,
                ActiveUsers = 142,
                InactiveUsers = 14,
                RecentLogins = 89,
                UsersByDepartment = new
                {
                    IT = 23,
                    Sales = 45,
                    CustomerService = 31,
                    Marketing = 28,
                    Finance = 15,
                    HR = 14
                },
                AvailableActions = new
                {
                    CanRead = true,
                    CanWrite = canWrite,
                    CanDelete = canDelete
                }
            }
        });
    }

    /// <summary>
    /// Write Permission Policy Example
    /// </summary>
    [HttpPost("user-management")]
    [Authorize(Policy = "CanWriteUsers")]
    public IActionResult CreateUser([FromBody] object userData)
    {
        var username = User.Identity?.Name;
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

        _logger.LogInformation("User creation attempted by {Username}", username);

        return Ok(new
        {
            Message = "User creation endpoint - requires 'users.write' permission",
            Username = username,
            Permissions = permissions,
            RequiredPolicy = "CanWriteUsers (Must have 'users.write' permission)",
            Action = "Create User",
            Result = "User would be created in a real implementation",
            Note = "This is a demo endpoint - no actual user was created"
        });
    }

    /// <summary>
    /// Region-Based Policy Example
    /// </summary>
    [HttpGet("north-america-data")]
    [Authorize(Policy = "NorthAmericaRegion")]
    public IActionResult GetNorthAmericaData()
    {
        var username = User.Identity?.Name;
        var region = User.FindFirst("region")?.Value;
        var department = User.FindFirst("department")?.Value;

        return Ok(new
        {
            Message = "This endpoint is accessible to North America and Global regions",
            Username = username,
            UserRegion = region,
            Department = department,
            RequiredPolicy = "NorthAmericaRegion (Region must be 'North America' or 'Global')",
            AccessLevel = "Regional",
            RegionalData = new
            {
                Region = "North America",
                Countries = new[] { "United States", "Canada", "Mexico" },
                SalesData = new
                {
                    Q1_Sales = "$2.1M",
                    Q2_Sales = "$2.4M", 
                    Q3_Sales = "$2.8M",
                    YTD_Growth = "+15%"
                },
                RegionalMetrics = new
                {
                    CustomerBase = 45230,
                    ActiveStores = 127,
                    EmployeeCount = 892
                }
            }
        });
    }

    /// <summary>
    /// Composite Policy Example (Multiple Requirements)
    /// </summary>
    [HttpGet("high-level-manager")]
    [Authorize(Policy = "HighLevelManager")]
    public IActionResult GetHighLevelManagerData()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

        return Ok(new
        {
            Message = "This endpoint requires composite authorization",
            Username = username,
            Roles = roles,
            SecurityLevel = securityLevel,
            Permissions = permissions,
            RequiredPolicy = "HighLevelManager (Security Level ≥ 3 + Admin/Manager role + specific permissions)",
            AccessLevel = "Executive Management",
            ExecutiveData = new
            {
                StrategicInitiatives = new[]
                {
                    "Digital transformation project - 65% complete",
                    "Market expansion to Asia-Pacific - Planning phase",
                    "AI/ML integration roadmap - Q2 2026 target"
                },
                FinancialSummary = new
                {
                    QuarterlyRevenue = "$12.8M",
                    ProfitMargin = "18.3%",
                    ROI = "24.7%",
                    MarketShare = "12.1%"
                },
                OrganizationalMetrics = new
                {
                    EmployeeSatisfaction = "87%",
                    RetentionRate = "92%",
                    ProductivityIndex = "1.23",
                    InnovationScore = "8.4/10"
                }
            }
        });
    }

    /// <summary>
    /// System Administrator Composite Policy
    /// </summary>
    [HttpGet("system-admin")]
    [Authorize(Policy = "SystemAdministrator")]
    public IActionResult GetSystemAdminData()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var securityLevel = User.FindFirst("security_level")?.Value;
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();

        return Ok(new
        {
            Message = "This endpoint requires System Administrator privileges",
            Username = username,
            Roles = roles,
            SecurityLevel = securityLevel,
            Permissions = permissions,
            RequiredPolicy = "SystemAdministrator (Admin role + Security Level ≥ 4 + system.admin permission)",
            AccessLevel = "System Administration",
            SystemData = new
            {
                ServerStatus = new
                {
                    WebServers = "3/3 Online",
                    DatabaseServers = "2/2 Online", 
                    CacheServers = "4/4 Online",
                    LoadBalancers = "2/2 Healthy"
                },
                SystemMetrics = new
                {
                    CPU_Usage = "34%",
                    Memory_Usage = "67%",
                    Disk_Usage = "45%",
                    Network_Throughput = "125 Mbps"
                },
                SecurityStatus = new
                {
                    LastSecurityScan = DateTime.UtcNow.AddHours(-6),
                    VulnerabilitiesFound = 0,
                    SecurityIncidents = 0,
                    ComplianceScore = "100%"
                },
                BackupStatus = new
                {
                    LastBackup = DateTime.UtcNow.AddHours(-2),
                    BackupSize = "2.3 TB",
                    BackupStatus = "Successful",
                    RetentionPeriod = "30 days"
                }
            }
        });
    }

    /// <summary>
    /// Senior Staff Policy Example (Multiple Requirements)
    /// </summary>
    [HttpGet("senior-staff")]
    [Authorize(Policy = "SeniorStaff")]
    public IActionResult GetSeniorStaffData()
    {
        var username = User.Identity?.Name;
        var securityLevel = User.FindFirst("security_level")?.Value;
        var department = User.FindFirst("department")?.Value;
        var hireDate = User.FindFirst("hire_date")?.Value;

        return Ok(new
        {
            Message = "This endpoint is for senior staff members",
            Username = username,
            SecurityLevel = securityLevel,
            Department = department,
            HireDate = hireDate,
            RequiredPolicy = "SeniorStaff (Security Level ≥ 2 + Specific departments + Authenticated)",
            AccessLevel = "Senior Staff",
            SeniorStaffData = new
            {
                StaffPrivileges = new[]
                {
                    "Access to senior staff meeting minutes",
                    "Company strategic planning documents",
                    "Advanced training opportunities",
                    "Mentorship program participation"
                },
                ResourceAccess = new
                {
                    KnowledgeBase = "Full access to internal documentation",
                    TrainingPortal = "Leadership and technical training modules",
                    MentorNetwork = "Senior staff mentorship program",
                    ExecutiveBriefings = "Monthly executive summary reports"
                }
            }
        });
    }
}