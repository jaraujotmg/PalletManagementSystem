using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Provides context information for the current user
    /// </summary>
    public class UserContext
    {
        private readonly IUserContextProvider _userContextProvider;
        private readonly WindowsAuthenticationService _windowsAuthService;
        private readonly ILogger<UserContext> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContext"/> class
        /// </summary>
        /// <param name="userContextProvider">The user context provider</param>
        /// <param name="windowsAuthService">The Windows authentication service</param>
        /// <param name="logger">The logger</param>
        public UserContext(
            IUserContextProvider userContextProvider,
            WindowsAuthenticationService windowsAuthService,
            ILogger<UserContext> logger)
        {
            _userContextProvider = userContextProvider ?? throw new ArgumentNullException(nameof(userContextProvider));
            _windowsAuthService = windowsAuthService ?? throw new ArgumentNullException(nameof(windowsAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current username
        /// </summary>
        /// <returns>The username</returns>
        public string GetUsername()
        {
            return _userContextProvider.GetCurrentUsername();
        }

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        /// <returns>The display name</returns>
        public async Task<string> GetDisplayNameAsync()
        {
            return await _userContextProvider.GetDisplayNameAsync();
        }

        /// <summary>
        /// Gets the current user's email
        /// </summary>
        /// <returns>The email</returns>
        public async Task<string> GetEmailAsync()
        {
            return await _userContextProvider.GetEmailAsync();
        }

        /// <summary>
        /// Checks if the current user is in a specified role
        /// </summary>
        /// <param name="role">The role name</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        public bool IsInRole(string role)
        {
            return _userContextProvider.IsInRole(role);
        }

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        /// <returns>An array of role names</returns>
        public async Task<string[]> GetRolesAsync()
        {
            return await _userContextProvider.GetRolesAsync();
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