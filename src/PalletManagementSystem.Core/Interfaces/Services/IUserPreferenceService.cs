using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for managing user preferences
    /// </summary>
    public interface IUserPreferenceService
    {
        /// <summary>
        /// Gets preferred division for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The preferred division</returns>
        Task<Division> GetPreferredDivisionAsync(string username);

        /// <summary>
        /// Sets preferred division for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="division">The division</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetPreferredDivisionAsync(string username, Division division);

        /// <summary>
        /// Gets preferred platform for a user within a division
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="division">The division</param>
        /// <returns>The preferred platform</returns>
        Task<Platform> GetPreferredPlatformAsync(string username, Division division);

        /// <summary>
        /// Sets preferred platform for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetPreferredPlatformAsync(string username, Division division, Platform platform);

        /// <summary>
        /// Gets whether touch mode is enabled for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>True if touch mode is enabled, false otherwise</returns>
        Task<bool> GetTouchModeEnabledAsync(string username);

        /// <summary>
        /// Sets whether touch mode is enabled for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="enabled">Whether touch mode is enabled</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetTouchModeEnabledAsync(string username, bool enabled);

        /// <summary>
        /// Gets the number of items per page for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The number of items per page</returns>
        Task<int> GetItemsPerPageAsync(string username);

        /// <summary>
        /// Sets the number of items per page for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="itemsPerPage">The number of items per page</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetItemsPerPageAsync(string username, int itemsPerPage);

        /// <summary>
        /// Gets the default pallet view for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The default pallet view</returns>
        Task<string> GetDefaultPalletViewAsync(string username);

        /// <summary>
        /// Sets the default pallet view for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="defaultView">The default view</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetDefaultPalletViewAsync(string username, string defaultView);

        /// <summary>
        /// Gets all preferences for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The user preferences</returns>
        Task<UserPreferencesDto> GetAllPreferencesAsync(string username);

        /// <summary>
        /// Sets all preferences for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="preferences">The user preferences</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetAllPreferencesAsync(string username, UserPreferencesDto preferences);

        /// <summary>
        /// Gets the session timeout for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The session timeout in minutes</returns>
        Task<int> GetSessionTimeoutAsync(string username);

        /// <summary>
        /// Sets the session timeout for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="timeoutMinutes">The session timeout in minutes</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetSessionTimeoutAsync(string username, int timeoutMinutes);
    }
}