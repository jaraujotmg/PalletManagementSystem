using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the user preference service
    /// </summary>
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly ILogger<UserPreferenceService> _logger;

        // In a real application, this would be stored in a database
        // For this implementation, we'll use an in-memory dictionary
        private static readonly Dictionary<string, UserPreferencesDto> _userPreferences =
            new Dictionary<string, UserPreferencesDto>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferenceService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public UserPreferenceService(ILogger<UserPreferenceService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Division> GetPreferredDivisionAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Parse division from string
                if (Enum.TryParse<Division>(preferences.PreferredDivision, out var division))
                {
                    return division;
                }

                // Default to Manufacturing
                return Division.MA;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting preferred division for user {username}");
                return Division.MA; // Default to Manufacturing
            }
        }

        /// <inheritdoc/>
        public async Task SetPreferredDivisionAsync(string username, Division division)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update division
                preferences.PreferredDivision = division.ToString();

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set preferred division for user {username} to {division}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting preferred division for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Platform> GetPreferredPlatformAsync(string username, Division division)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Parse platform from string
                if (Enum.TryParse<Platform>(preferences.PreferredPlatform, out var platform))
                {
                    // Check if the platform is valid for the division
                    if (IsValidPlatformForDivision(platform, division))
                    {
                        return platform;
                    }
                }

                // Return default platform for division
                return GetDefaultPlatformForDivision(division);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting preferred platform for user {username}");
                return GetDefaultPlatformForDivision(division);
            }
        }

        /// <inheritdoc/>
        public async Task SetPreferredPlatformAsync(string username, Division division, Platform platform)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            // Validate platform for division
            if (!IsValidPlatformForDivision(platform, division))
            {
                throw new ArgumentException($"Platform {platform} is not valid for division {division}", nameof(platform));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update platform
                preferences.PreferredPlatform = platform.ToString();

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set preferred platform for user {username} to {platform}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting preferred platform for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> GetTouchModeEnabledAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                return preferences.TouchModeEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting touch mode preference for user {username}");
                return false; // Default to disabled
            }
        }

        /// <inheritdoc/>
        public async Task SetTouchModeEnabledAsync(string username, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update touch mode
                preferences.TouchModeEnabled = enabled;

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set touch mode for user {username} to {enabled}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting touch mode preference for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetItemsPerPageAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                return preferences.ItemsPerPage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting items per page preference for user {username}");
                return 20; // Default to 20 items per page
            }
        }

        /// <inheritdoc/>
        public async Task SetItemsPerPageAsync(string username, int itemsPerPage)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (itemsPerPage <= 0)
            {
                throw new ArgumentException("Items per page must be greater than zero", nameof(itemsPerPage));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update items per page
                preferences.ItemsPerPage = itemsPerPage;

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set items per page for user {username} to {itemsPerPage}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting items per page preference for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetDefaultPalletViewAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                return preferences.DefaultPalletView;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default pallet view preference for user {username}");
                return "all"; // Default to showing all pallets
            }
        }

        /// <inheritdoc/>
        public async Task SetDefaultPalletViewAsync(string username, string defaultView)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(defaultView))
            {
                throw new ArgumentException("Default view cannot be null or empty", nameof(defaultView));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update default pallet view
                preferences.DefaultPalletView = defaultView;

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set default pallet view for user {username} to {defaultView}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default pallet view preference for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserPreferencesDto> GetAllPreferencesAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                return await GetUserPreferencesAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all preferences for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task SetAllPreferencesAsync(string username, UserPreferencesDto preferences)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (preferences == null)
            {
                throw new ArgumentNullException(nameof(preferences));
            }

            try
            {
                // Validate preferences
                ValidatePreferences(preferences);

                // Update username
                preferences.Username = username;

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set all preferences for user {username}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting all preferences for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetSessionTimeoutAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                return preferences.SessionTimeoutMinutes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting session timeout preference for user {username}");
                return 30; // Default to 30 minutes
            }
        }

        /// <inheritdoc/>
        public async Task SetSessionTimeoutAsync(string username, int timeoutMinutes)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (timeoutMinutes <= 0)
            {
                throw new ArgumentException("Session timeout must be greater than zero", nameof(timeoutMinutes));
            }

            try
            {
                // Get user preferences or create default
                var preferences = await GetUserPreferencesAsync(username);

                // Update session timeout
                preferences.SessionTimeoutMinutes = timeoutMinutes;

                // Save preferences
                await SaveUserPreferencesAsync(username, preferences);

                _logger.LogInformation($"Set session timeout for user {username} to {timeoutMinutes} minutes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting session timeout preference for user {username}");
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Gets user preferences or creates default preferences if not found
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The user preferences</returns>
        private async Task<UserPreferencesDto> GetUserPreferencesAsync(string username)
        {
            // In a real application, this would retrieve from a database
            // For this implementation, we'll use an in-memory dictionary
            if (_userPreferences.TryGetValue(username, out var preferences))
            {
                return preferences;
            }

            // Create default preferences
            var defaultPreferences = new UserPreferencesDto
            {
                Username = username,
                PreferredDivision = Division.MA.ToString(),
                PreferredPlatform = Platform.TEC1.ToString(),
                TouchModeEnabled = false,
                ItemsPerPage = 20,
                DefaultPalletView = "all",
                DefaultPalletListPrinter = "HP LaserJet 4200 - Office",
                DefaultItemLabelPrinter = "Zebra ZT410 - Warehouse",
                ShowConfirmationPrompts = true,
                AutoPrintPalletList = true,
                UseSpecialPrinterForSpecialClients = true,
                SessionTimeoutMinutes = 30,
                RememberDivisionAndPlatform = true,
                AutoRefreshPalletList = false,
                AutoRefreshIntervalSeconds = 60,
                ShowBrowserNotifications = true
            };

            // Save default preferences
            await SaveUserPreferencesAsync(username, defaultPreferences);

            return defaultPreferences;
        }

        /// <summary>
        /// Saves user preferences
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="preferences">The user preferences</param>
        private async Task SaveUserPreferencesAsync(string username, UserPreferencesDto preferences)
        {
            // In a real application, this would save to a database
            // For this implementation, we'll use an in-memory dictionary
            _userPreferences[username] = preferences;

            await Task.CompletedTask; // To make async method
        }

        /// <summary>
        /// Validates user preferences
        /// </summary>
        /// <param name="preferences">The user preferences</param>
        /// <exception cref="ArgumentException">Thrown when preferences are invalid</exception>
        private void ValidatePreferences(UserPreferencesDto preferences)
        {
            // Validate division
            if (!Enum.TryParse<Division>(preferences.PreferredDivision, out var division))
            {
                preferences.PreferredDivision = Division.MA.ToString();
            }

            // Validate platform
            if (!Enum.TryParse<Platform>(preferences.PreferredPlatform, out var platform) ||
                !IsValidPlatformForDivision(platform, division))
            {
                preferences.PreferredPlatform = GetDefaultPlatformForDivision(division).ToString();
            }

            // Validate items per page
            if (preferences.ItemsPerPage <= 0)
            {
                preferences.ItemsPerPage = 20;
            }

            // Validate session timeout
            if (preferences.SessionTimeoutMinutes <= 0)
            {
                preferences.SessionTimeoutMinutes = 30;
            }

            // Validate auto refresh interval
            if (preferences.AutoRefreshIntervalSeconds <= 0)
            {
                preferences.AutoRefreshIntervalSeconds = 60;
            }
        }

        /// <summary>
        /// Checks if a platform is valid for a division
        /// </summary>
        /// <param name="platform">The platform</param>
        /// <param name="division">The division</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool IsValidPlatformForDivision(Platform platform, Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return platform == Platform.TEC1 || platform == Platform.TEC2 || platform == Platform.TEC4I;

                case Division.TC:
                    return platform == Platform.TEC1 || platform == Platform.TEC3 || platform == Platform.TEC5;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the default platform for a division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>The default platform</returns>
        private Platform GetDefaultPlatformForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return Platform.TEC1;

                case Division.TC:
                    return Platform.TEC1;

                default:
                    return Platform.TEC1;
            }
        }

        #endregion
    }
}