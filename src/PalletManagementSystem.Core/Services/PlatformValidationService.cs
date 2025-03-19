using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Models
{
    /// <summary>
    /// Service for validating platform and division combinations
    /// </summary>
    public static class PlatformValidationService
    {
        /// <summary>
        /// Determines if a platform is valid for a division
        /// </summary>
        /// <param name="platform">The platform to check</param>
        /// <param name="division">The division</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidPlatformForDivision(Platform platform, Division division)
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
        /// <returns>The default platform for the specified division</returns>
        public static Platform GetDefaultPlatformForDivision(Division division)
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

        /// <summary>
        /// Gets all platforms valid for a specific division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>Array of valid platforms</returns>
        public static Platform[] GetPlatformsForDivision(Division division)
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
    }
}