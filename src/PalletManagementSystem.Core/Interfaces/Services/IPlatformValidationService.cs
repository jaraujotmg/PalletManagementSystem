using PalletManagementSystem.Core.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for validating and managing platform and division relationships
    /// </summary>
    public interface IPlatformValidationService
    {
        /// <summary>
        /// Determines if a platform is valid for a division
        /// </summary>
        /// <param name="platform">The platform to validate</param>
        /// <param name="division">The division to validate against</param>
        /// <returns>True if the platform is valid for the division, false otherwise</returns>
        Task<bool> IsValidPlatformForDivisionAsync(Platform platform, Division division);

        /// <summary>
        /// Gets the default platform for a division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>The default platform for the specified division</returns>
        Task<Platform> GetDefaultPlatformForDivisionAsync(Division division);

        /// <summary>
        /// Gets all platforms available for a division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>Collection of platforms valid for the division</returns>
        Task<IEnumerable<Platform>> GetPlatformsForDivisionAsync(Division division);
    }
}