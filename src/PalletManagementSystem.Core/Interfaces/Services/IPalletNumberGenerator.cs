using System.Threading.Tasks;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for generating and validating pallet numbers
    /// </summary>
    public interface IPalletNumberGenerator
    {
        /// <summary>
        /// Generates a new temporary pallet number
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>The temporary pallet number</returns>
        Task<PalletNumber> GenerateTemporaryNumberAsync(Division division);

        /// <summary>
        /// Generates a new permanent pallet number
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>The permanent pallet number</returns>
        Task<PalletNumber> GeneratePermanentNumberAsync(Division division);

        /// <summary>
        /// Checks if a pallet number already exists
        /// </summary>
        /// <param name="palletNumber">The pallet number to check</param>
        /// <returns>True if the pallet number exists, false otherwise</returns>
        Task<bool> PalletNumberExistsAsync(string palletNumber);

        /// <summary>
        /// Validates a pallet number format
        /// </summary>
        /// <param name="palletNumber">The pallet number to validate</param>
        /// <param name="division">The division</param>
        /// <returns>True if the format is valid, false otherwise</returns>
        static bool ValidatePalletNumberFormat(string palletNumber, Division division) => throw new System.NotImplementedException();
    }
}