// PalletManagementSystem.Web/Identity/AspNetUserContextProvider.cs
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Identity;
using PalletManagementSystem.Core.Interfaces;
using PalletManagementSystem.Infrastructure.Identity;
using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace PalletManagementSystem.Web.Identity
{
    /// <summary>
    /// ASP.NET implementation of the user context provider
    /// </summary>
    public class AspNetUserContextProvider : BaseUserContextProvider
    {
        private readonly WindowsAuthenticationService _windowsAuthService;

        /// <summary>
        /// Initializes a new instance of AspNetUserContextProvider
        /// </summary>
        /// <param name="windowsAuthService">Windows authentication service</param>
        /// <param name="logger">Logger</param>
        public AspNetUserContextProvider(
            WindowsAuthenticationService windowsAuthService,
            ILogger<AspNetUserContextProvider> logger)
            : base(logger)
        {
            _windowsAuthService = windowsAuthService ?? throw new ArgumentNullException(nameof(windowsAuthService));
        }

        /// <inheritdoc/>
        public override string GetCurrentUsername()
        {
            try
            {
                var identity = HttpContext.Current?.User?.Identity;

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

        // Implement other methods similarly
        public override async Task<string> GetDisplayNameAsync()
        {
            try
            {
                var identity = HttpContext.Current?.User?.Identity;

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

        public override async Task<string> GetEmailAsync()
        {
            try
            {
                var identity = HttpContext.Current?.User?.Identity;

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

                // Fallback to username with example domain
                var username = GetCurrentUsername();
                return $"{username}@example.com";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email");
                return string.Empty;
            }
        }

        public override bool IsInRole(string role)
        {
            try
            {
                return HttpContext.Current?.User?.IsInRole(role) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user is in role {role}");
                return false;
            }
        }

        public override async Task<string[]> GetRolesAsync()
        {
            try
            {
                var identity = HttpContext.Current?.User?.Identity;

                if (identity == null || !identity.IsAuthenticated)
                {
                    return Array.Empty<string>();
                }

                // Try to get roles from Windows identity
                if (identity is WindowsIdentity windowsIdentity)
                {
                    return await _windowsAuthService.GetUserRolesAsync(windowsIdentity);
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
    }
}