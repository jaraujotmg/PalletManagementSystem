using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Services
{
    /// <summary>
    /// Implementation of the platform validation service
    /// </summary>
    public class PlatformValidationService : IPlatformValidationService
    {
        private readonly ILogger<PlatformValidationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformValidationService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public PlatformValidationService(ILogger<PlatformValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<bool> IsValidPlatformForDivisionAsync(Platform platform, Division division)
        {
            try
            {
                // Return result as an async operation for consistency
                return await Task.FromResult(IsValidPlatformForDivision(platform, division));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating platform {platform} for division {division}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Platform> GetDefaultPlatformForDivisionAsync(Division division)
        {
            try
            {
                // Return result as an async operation for consistency
                return await Task.FromResult(GetDefaultPlatformForDivision(division));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default platform for division {division}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Platform>> GetPlatformsForDivisionAsync(Division division)
        {
            try
            {
                // Return result as an async operation for consistency
                return await Task.FromResult(GetPlatformsForDivision(division));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting platforms for division {division}");
                throw;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Determines if a platform is valid for a division
        /// </summary>
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
        private Platform GetDefaultPlatformForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                case Division.TC:
                    return Platform.TEC1;
                default:
                    return Platform.TEC1;
            }
        }

        /// <summary>
        /// Gets all platforms valid for a specific division
        /// </summary>
        private Platform[] GetPlatformsForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return new[] { Platform.TEC1, Platform.TEC2, Platform.TEC4I };
                case Division.TC:
                    return new[] { Platform.TEC1, Platform.TEC3, Platform.TEC5 };
                default:
                    return new[] { Platform.TEC1 };
            }
        }

        #endregion
    }
}