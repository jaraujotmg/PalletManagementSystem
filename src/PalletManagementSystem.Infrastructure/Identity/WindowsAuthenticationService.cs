using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Service for Windows Authentication operations
    /// </summary>
    public class WindowsAuthenticationService
    {
        private readonly ILogger<WindowsAuthenticationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAuthenticationService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public WindowsAuthenticationService(ILogger<WindowsAuthenticationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets user details from Active Directory
        /// </summary>
        /// <param name="identity">The Windows identity</param>
        /// <returns>A tuple with username, display name, and email</returns>
        public async Task<(string Username, string DisplayName, string Email)> GetUserDetailsAsync(WindowsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            try
            {
                // Extract username from identity
                var username = identity.Name.Split('\\').Length > 1
                    ? identity.Name.Split('\\')[1]
                    : identity.Name;

                // Simulate an async operation
                await Task.Delay(1).ConfigureAwait(false); ;

                // Try to get user details from Active Directory
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    try
                    {
                        var user = UserPrincipal.FindByIdentity(context, username);
                        if (user != null)
                        {
                            return (
                                user.SamAccountName,
                                user.DisplayName,
                                user.EmailAddress
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting user details from Active Directory. Using fallback method.");
                    }
                }

                // Fallback if Active Directory query fails
                return (
                    username,
                    identity.Name,
                    $"{username}@example.com"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user details for {identity.Name}");
                throw;
            }
        }

        /// <summary>
        /// Checks if a user is in a specific Windows group
        /// </summary>
        /// <param name="identity">The Windows identity</param>
        /// <param name="groupName">The group name</param>
        /// <returns>True if the user is in the group, false otherwise</returns>
        public async Task<bool> IsUserInGroupAsync(WindowsIdentity identity, string groupName)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Group name cannot be null or empty", nameof(groupName));
            }

            try
            {
                // Simulate an async operation
                await Task.Delay(1).ConfigureAwait(false); 
                    
                // Check if the identity is in the specified group
                var isInGroup = identity.Groups != null &&
                    identity.Groups.OfType<SecurityIdentifier>()
                    .Any(sid =>
                    {
                        try
                        {
                            var ntAccount = sid.Translate(typeof(NTAccount));
                            return ntAccount.Value.EndsWith(groupName, StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            // Ignore errors that could happen during name translation
                            return false;
                        }
                    });

                _logger.LogInformation($"User {identity.Name} {(isInGroup ? "is" : "is not")} in group {groupName}");

                return isInGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user {identity.Name} is in group {groupName}");
                throw;
            }
        }

        /// <summary>
        /// Gets the roles for a user
        /// </summary>
        /// <param name="identity">The Windows identity</param>
        /// <returns>An array of role names</returns>
        public async Task<string[]> GetUserRolesAsync(WindowsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            try
            {
                // Define role mapping to Windows groups
                var roleGroupMap = new Dictionary<string, string>
                {
                    { "Administrator", "PalletSystem_Admins" },
                    { "Editor", "PalletSystem_Editors" },
                    { "Viewer", "PalletSystem_Viewers" }
                };

                // Check which groups the user is in
                var roles = new List<string>();

                foreach (var role in roleGroupMap)
                {
                    if (await IsUserInGroupAsync(identity, role.Value))
                    {
                        roles.Add(role.Key);
                    }
                }

                // Ensure at least Viewer role is assigned
                if (!roles.Any())
                {
                    roles.Add("Viewer");
                }

                return roles.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting roles for user {identity.Name}");
                throw;
            }
        }
    }
}