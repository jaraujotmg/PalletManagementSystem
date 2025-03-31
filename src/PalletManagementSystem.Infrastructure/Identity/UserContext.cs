// src/PalletManagementSystem.Infrastructure/Identity/UserContext.cs
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Linq; // Keep Linq using
using System.Security.Principal;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Interfaces; // For IUserSessionContext

namespace PalletManagementSystem.Infrastructure.Identity
{
    public class UserContext : IUserContext
    {
        private readonly WindowsAuthenticationService _windowsAuthService;
        private readonly ILogger<UserContext> _logger;
        private readonly WindowsIdentity _windowsIdentity;
        private readonly IUserSessionContext _userSessionContext;

        public UserContext(
            WindowsAuthenticationService windowsAuthService,
            ILogger<UserContext> logger,
            IUserSessionContext userSessionContext)
        {
            _windowsAuthService = windowsAuthService ?? throw new ArgumentNullException(nameof(windowsAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userSessionContext = userSessionContext ?? throw new ArgumentNullException(nameof(userSessionContext));

            try { _windowsIdentity = WindowsIdentity.GetCurrent(); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to get current Windows identity"); }
        }

        // --- Keep sync/async methods for identity details ---
        public string GetUsername()
        {
            try
            {
                var username = _windowsIdentity?.Name?.Split('\\').Length > 1
                    ? _windowsIdentity.Name.Split('\\')[1]
                    : _windowsIdentity?.Name;
                return !string.IsNullOrEmpty(username) ? username : Environment.UserName;
            }
            catch (Exception ex) { _logger.LogError(ex, "Error getting username"); return Environment.UserName; }
        }

        public async Task<string> GetDisplayNameAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(_windowsIdentity);
                    return !string.IsNullOrEmpty(userDetails.DisplayName) ? userDetails.DisplayName : GetUsername();
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error getting display name"); }
            return GetUsername();
        }

        public async Task<string> GetEmailAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    var userDetails = await _windowsAuthService.GetUserDetailsAsync(_windowsIdentity);
                    return !string.IsNullOrEmpty(userDetails.Email) ? userDetails.Email : $"{GetUsername()}@example.com";
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error getting email"); }
            return $"{GetUsername()}@example.com";
        }

        // --- Keep GetRolesAsync as is ---
        public async Task<string[]> GetRolesAsync()
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    return await _windowsAuthService.GetUserRolesAsync(_windowsIdentity);
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error getting roles"); }
            return new[] { "Viewer" };
        }


        // --- Implement ASYNC permission checks ---

        public async Task<bool> IsInRoleAsync(string role) // Now async Task<bool>
        {
            try
            {
                if (_windowsIdentity != null)
                {
                    // Use await instead of .Result
                    var roles = await GetRolesAsync();
                    return System.Linq.Enumerable.Contains(roles, role, StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in role {role}");
            }
            // Fallback check if getting roles failed or no identity
            return role.Equals("Viewer", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> CanEditPalletsAsync() =>
            await IsInRoleAsync("Administrator") || await IsInRoleAsync("Editor");

        public async Task<bool> CanClosePalletsAsync() =>
            await IsInRoleAsync("Administrator") || await IsInRoleAsync("Editor");

        public async Task<bool> CanEditItemsAsync() =>
            await IsInRoleAsync("Administrator") || await IsInRoleAsync("Editor");

        public async Task<bool> CanMoveItemsAsync() =>
             await IsInRoleAsync("Administrator") || await IsInRoleAsync("Editor");

        // --- Keep sync methods for Division/Platform ---
        public Division GetDivision()
        {
            try { return _userSessionContext.GetCurrentDivision(); }
            catch (Exception ex) { _logger.LogError(ex, "Error getting division via IUserSessionContext"); return Division.MA; }
        }

        public Platform GetPlatform()
        {
            try { return _userSessionContext.GetCurrentPlatform(); }
            catch (Exception ex) { _logger.LogError(ex, "Error getting platform via IUserSessionContext"); return Platform.TEC1; }
        }
    }
}