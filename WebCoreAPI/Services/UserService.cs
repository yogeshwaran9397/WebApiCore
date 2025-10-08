using WebCoreAPI.Models.Auth;

namespace WebCoreAPI.Services;

/// <summary>
/// User Service for managing users (In-Memory implementation for demo)
/// In production, this would connect to a database
/// </summary>
public class UserService
{
    private readonly List<User> _users;
    private readonly ILogger<UserService> _logger;
    private static int _nextId = 1;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        _users = InitializeUsers();
    }

    /// <summary>
    /// Initialize demo users with different roles and claims
    /// </summary>
    private List<User> InitializeUsers()
    {
        return new List<User>
        {
            // Admin User
            new User
            {
                Id = _nextId++,
                Username = "admin",
                Password = "admin123", // In real apps, use hashed passwords
                Email = "admin@bookstore.com",
                FirstName = "Admin",
                LastName = "User",
                Department = "IT",
                SecurityLevel = 5,
                Region = "Global",
                Roles = new List<string> { "Admin", "User", "Manager" },
                Permissions = new List<string> { "users.read", "users.write", "users.delete", "books.read", "books.write", "books.delete", "reports.read", "system.admin" },
                Claims = new Dictionary<string, string>
                {
                    ["employee_id"] = "EMP001",
                    ["hire_date"] = "2020-01-01",
                    ["salary_band"] = "Executive",
                    ["clearance_level"] = "TOP_SECRET"
                }
            },

            // Manager User
            new User
            {
                Id = _nextId++,
                Username = "manager",
                Password = "manager123",
                Email = "manager@bookstore.com",
                FirstName = "John",
                LastName = "Manager",
                Department = "Sales",
                SecurityLevel = 3,
                Region = "North America",
                Roles = new List<string> { "Manager", "User" },
                Permissions = new List<string> { "users.read", "books.read", "books.write", "reports.read", "team.manage" },
                Claims = new Dictionary<string, string>
                {
                    ["employee_id"] = "EMP002",
                    ["hire_date"] = "2021-03-15",
                    ["salary_band"] = "Manager",
                    ["team_size"] = "10"
                }
            },

            // Regular User
            new User
            {
                Id = _nextId++,
                Username = "user",
                Password = "user123",
                Email = "user@bookstore.com",
                FirstName = "Jane",
                LastName = "Smith",
                Department = "Sales",
                SecurityLevel = 1,
                Region = "North America",
                Roles = new List<string> { "User" },
                Permissions = new List<string> { "books.read", "profile.read", "profile.write" },
                Claims = new Dictionary<string, string>
                {
                    ["employee_id"] = "EMP003",
                    ["hire_date"] = "2022-06-01",
                    ["salary_band"] = "Associate"
                }
            },

            // Customer Service User
            new User
            {
                Id = _nextId++,
                Username = "support",
                Password = "support123",
                Email = "support@bookstore.com",
                FirstName = "Mike",
                LastName = "Support",
                Department = "Customer Service",
                SecurityLevel = 2,
                Region = "Europe",
                Roles = new List<string> { "Support", "User" },
                Permissions = new List<string> { "books.read", "customers.read", "orders.read", "tickets.write" },
                Claims = new Dictionary<string, string>
                {
                    ["employee_id"] = "EMP004",
                    ["hire_date"] = "2023-01-10",
                    ["salary_band"] = "Support",
                    ["languages"] = "EN,FR,DE"
                }
            },

            // Guest User (limited access)
            new User
            {
                Id = _nextId++,
                Username = "guest",
                Password = "guest123",
                Email = "guest@bookstore.com",
                FirstName = "Guest",
                LastName = "User",
                Department = "None",
                SecurityLevel = 0,
                Region = "Global",
                Roles = new List<string> { "Guest" },
                Permissions = new List<string> { "books.read" },
                Claims = new Dictionary<string, string>
                {
                    ["account_type"] = "Guest",
                    ["access_level"] = "Limited"
                }
            }
        };
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        // In production, compare hashed passwords
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
            u.Password == password && 
            u.IsActive);

        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            _logger.LogInformation("User {Username} authenticated successfully", username);
        }
        else
        {
            _logger.LogWarning("Authentication failed for username: {Username}", username);
        }

        return await Task.FromResult(user);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetByIdAsync(int id)
    {
        return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id && u.IsActive));
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Task.FromResult(_users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive));
    }

    /// <summary>
    /// Register new user
    /// </summary>
    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        if (_users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Username already exists");
        }

        // Check if email already exists
        if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Email already exists");
        }

        var user = new User
        {
            Id = _nextId++,
            Username = request.Username,
            Email = request.Email,
            Password = request.Password, // In production, hash the password
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            SecurityLevel = 1, // Default security level
            Region = "Unknown", // Default region
            Roles = request.Roles.Any() ? request.Roles : new List<string> { "User" }, // Default role
            Permissions = new List<string> { "books.read", "profile.read", "profile.write" }, // Default permissions
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users.Add(user);
        _logger.LogInformation("New user registered: {Username}", user.Username);

        return await Task.FromResult(user);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await Task.FromResult(_users.Where(u => u.IsActive).ToList());
    }

    /// <summary>
    /// Update user
    /// </summary>
    public async Task<User?> UpdateUserAsync(int id, User updatedUser)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return null;

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.Department = updatedUser.Department;
        user.SecurityLevel = updatedUser.SecurityLevel;
        user.Region = updatedUser.Region;
        user.Roles = updatedUser.Roles;
        user.Permissions = updatedUser.Permissions;
        user.Claims = updatedUser.Claims;

        _logger.LogInformation("User {Username} updated", user.Username);
        return await Task.FromResult(user);
    }
}