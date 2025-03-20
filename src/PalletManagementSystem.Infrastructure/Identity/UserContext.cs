using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Provides context information for the current user
    /// </summary>
    public class UserContext : IUserContext
    {
        private readonly WindowsAuthenticationService _windowsAuthService;
        private readonly ILogger<UserContext> _logger;
        private readonly WindowsIdentity _windowsIdentity;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserContext"/> class
        /// </summary>
        /// <param name="windowsAuthService">The Windows authentication service</param>
        /// <param name="logger">The logger</param>
        public UserContext(
            WindowsAuthenticationService windowsAuthService,
            ILogger<UserContext> logger)
        {
            _windowsAuthService = windowsAuthService ?? throw new ArgumentNullException(nameof(windowsAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                _windowsIdentity = WindowsIdentity.GetCurrent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get current Windows identity");
            }
        }

        /// <inheritdoc/>
        public string GetUsername()
        {
            try
            {
                return _windowsIdentity?.Name?.Split('\\').Length > 1
                    ? _windowsIdentity.Name.Split('\\')[1]
                    : _windowsIdentity?.Name ?? Environment.UserName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting username");
                return Environment.UserName;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetDisplayNameAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(_windowsIdentity);
                    return userDetails.DisplayName;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting display name");
            }

            return GetUsername();
        }

        /// <inheritdoc/>
        public async Task<string> GetEmailAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(_windowsIdentity);
                    return userDetails.Email;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email");
            }

            return $"{GetUsername()}@example.com";
        }

        /// <inheritdoc/>
        public bool IsInRole(string role)
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    // This is synchronous for simplicity
                    var isInGroup = _windowsAuthService.IsUserInGroupAsync(_windowsIdentity, $"PalletSystem_{role}s").Result;
                    return isInGroup;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in role {role}");
            }

            // Default to Viewer role only
            return role == "Viewer";
        }

        /// <inheritdoc/>
        public async Task<string[]> GetRolesAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    return await _windowsAuthService.GetUserRolesAsync(_windowsIdentity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
            }

            return new[] { "Viewer" };
        }

        /// <inheritdoc/>
        public bool CanEditPallets()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <inheritdoc/>
        public bool CanClosePallets()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <inheritdoc/>
        public bool CanEditItems()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <inheritdoc/>
        public bool CanMoveItems()
        {
            return IsInRole("Administrator") || IsInRole("Editor");
        }

        /// <inheritdoc/>
        public Division GetDivision()
        {
            try
            {
                // In a real application, this would get the division from the session
                // For now, default to Manufacturing
                return Division.MA;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting division");
                return Division.MA;
            }
        }

        /// <inheritdoc/>
        public Platform GetPlatform()
        {
            try
            {
                // In a real application, this would get the platform from the session
                // For now, default to TEC1
                return Platform.TEC1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform");
                return Platform.TEC1;
            }
        }
    }
}