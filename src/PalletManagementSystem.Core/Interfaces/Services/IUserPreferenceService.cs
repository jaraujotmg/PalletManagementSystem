using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for user preference operations
    /// </summary>
    public interface IUserPreferenceService
    {
        /// <summary>
        /// Gets the user's preferred division
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The preferred division</returns>
        Task<Division> GetPreferredDivisionAsync(string username);

        /// <summary>
        /// Sets the user's preferred division
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="division">The division</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetPreferredDivisionAsync(string username, Division division);

        /// <summary>
        /// Gets the user's preferred platform for a division
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="division">The division</param>
        /// <returns>The preferred platform</returns>
        Task<Platform> GetPreferredPlatformAsync(string username, Division division);

        /// <summary>
        /// Sets the user's preferred platform for a division
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetPreferredPlatformAsync(string username, Division division, Platform platform);

        /// <summary>
        /// Gets the user's touch mode preference
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>True if touch mode is enabled, false otherwise</returns>
        Task<bool> GetTouchModeEnabledAsync(string username);

        /// <summary>
        /// Sets the user's touch mode preference
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="enabled">True to enable touch mode, false to disable</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetTouchModeEnabledAsync(string username, bool enabled);

        /// <summary>
        /// Gets the user's preference for items per page
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The number of items per page</returns>
        Task<int> GetItemsPerPageAsync(string username);

        /// <summary>
        /// Sets the user's preference for items per page
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="itemsPerPage">The number of items per page</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetItemsPerPageAsync(string username, int itemsPerPage);

        /// <summary>
        /// Gets the user's default pallet view preference
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The default pallet view</returns>
        Task<string> GetDefaultPalletViewAsync(string username);

        /// <summary>
        /// Sets the user's default pallet view preference
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="defaultView">The default pallet view</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetDefaultPalletViewAsync(string username, string defaultView);

        /// <summary>
        /// Gets all preferences for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The user preferences DTO</returns>
        Task<UserPreferencesDto> GetAllPreferencesAsync(string username);

        /// <summary>
        /// Sets all preferences for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="preferences">The user preferences DTO</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetAllPreferencesAsync(string username, UserPreferencesDto preferences);

        /// <summary>
        /// Gets the session timeout value for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The session timeout in minutes</returns>
        Task<int> GetSessionTimeoutAsync(string username);

        /// <summary>
        /// Sets the session timeout value for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="timeoutMinutes">The session timeout in minutes</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetSessionTimeoutAsync(string username, int timeoutMinutes);
    }
}