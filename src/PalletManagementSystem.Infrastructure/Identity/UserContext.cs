using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Provides context information for the current user
    /// </summary>
    public class UserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WindowsAuthenticationService _windowsAuthService;
        private readonly ILogger<UserContext> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContext"/> class
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        /// <param name="windowsAuthService">The Windows authentication service</param>
        /// <param name="logger">The logger</param>
        public UserContext(
            IHttpContextAccessor httpContextAccessor,
            WindowsAuthenticationService windowsAuthService,
            ILogger<UserContext> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _windowsAuthService = windowsAuthService ?? throw new ArgumentNullException(nameof(windowsAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current username
        /// </summary>
        /// <returns>The username</returns>
        public string GetUsername()
        {
            try
            {
                var identity = _httpContextAccessor.HttpContext?.User?.Identity;

                if (identity == null || !identity.IsAuthenticated)
                {
                    return "Unknown";
                }

                // Extract username from Windows identity
                if (identity is WindowsIdentity windowsIdentity)
                {
                    var username = windowsIdentity.Name.Split('\\').Length > 1
                        ? windowsIdentity.Name.Split('\\')[1]
                        : windowsIdentity.Name;

                    return username;
                }

                // Fallback to identity name
                return identity.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting username");
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        /// <returns>The display name</returns>
        public async Task<string> GetDisplayNameAsync()
        {
            try
            {
                var identity = _httpContextAccessor.HttpContext?.User?.Identity;

                if (identity == null || !identity.IsAuthenticated)
                {
                    return "Unknown User";
                }

                // Try to get display name from Windows identity
                if (identity is WindowsIdentity windowsIdentity)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(windowsIdentity);
                    return !string.IsNullOrEmpty(userDetails.DisplayName)
                        ? userDetails.DisplayName
                        : windowsIdentity.Name;
                }

                // Fallback to identity name
                return identity.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting display name");
                return "Unknown User";
            }
        }

        /// <summary>
        /// Gets the current user's email
        /// </summary>
        /// <returns>The email</returns>
        public async Task<string> GetEmailAsync()
        {
            try
            {
                var identity = _httpContextAccessor.HttpContext?.User?.Identity;

                if (identity == null || !identity.IsAuthenticated)
                {
                    return string.Empty;
                }

                // Try to get email from Windows identity
                if (identity is WindowsIdentity windowsIdentity)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(windowsIdentity);
                    return userDetails.Email;
                }

                // Try to get email from claims
                var emailClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email);
                if (emailClaim != null)
                {
                    return emailClaim.Value;
                }

                // Fallback to username with example domain
                var username = GetUsername();
                return $"{username}@example.com";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email");
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if the current user is in a specified role
        /// </summary>
        /// <param name="role">The role name</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        public bool IsInRole(string role)
        {
            try
            {
                return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in role {role}");
                return false;
            }
        }

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        /// <returns>An array of role names</returns>
        public async Task<string[]> GetRolesAsync()
        {
            try
            {
                var identity = _httpContextAccessor.HttpContext?.User?.Identity;

                if (identity == null || !identity.IsAuthenticated)
                {
                    return Array.Empty<string>();
                }

                // Try to get roles from Windows identity
                if (identity is WindowsIdentity windowsIdentity)
                {
                    return await _windowsAuthService.GetUserRolesAsync(windowsIdentity);
                }

                // Try to get roles from claims
                var roleClaims = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);
                if (roleClaims != null && roleClaims.Any())
                {
                    return roleClaims.Select(c => c.Value).ToArray();
                }

                // Fallback to Viewer role
                return new[] { "Viewer" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return new[] { "Viewer" };
            }
        }

        /// <summary>
        /// Checks if the current user can edit pallets
        /// </summary>
        /// <returns>True if the user can edit pallets, false otherwise</returns>
        public bool CanEditPallets()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <summary>
        /// Checks if the current user can close pallets
        /// </summary>
        /// <returns>True if the user can close pallets, false otherwise</returns>
        public bool CanClosePallets()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <summary>
        /// Checks if the current user can edit items
        /// </summary>
        /// <returns>True if the user can edit items, false otherwise</returns>
        public bool CanEditItems()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <summary>
        /// Checks if the current user can move items
        /// </summary>
        /// <returns>True if the user can move items, false otherwise</returns>
        public bool CanMoveItems()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <summary>
        /// Gets the current session's division
        /// </summary>
        /// <returns>The division</returns>
        public Division GetDivision()
        {
            try
            {
                // In a real application, this would get the division from the session
                // For this implementation, we'll use a default value
                var divisionClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Division");
                if (divisionClaim != null && Enum.TryParse<Division>(divisionClaim.Value, out var division))
                {
                    return division;
                }

                return Division.MA; // Default to Manufacturing
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting division");
                return Division.MA; // Default to Manufacturing
            }
        }

        /// <summary>
        /// Gets the current session's platform
        /// </summary>
        /// <returns>The platform</returns>
        public Platform GetPlatform()
        {
            try
            {
                // In a real application, this would get the platform from the session
                // For this implementation, we'll use a default value
                var platformClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("Platform");
                if (platformClaim != null && Enum.TryParse<Platform>(platformClaim.Value, out var platform))
                {
                    return platform;
                }

                return Platform.TEC1; // Default to TEC1
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform");
                return Platform.TEC1; // Default to TEC1
            }
        }
    }
}