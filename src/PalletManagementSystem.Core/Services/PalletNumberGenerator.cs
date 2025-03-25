using System;
using System.Linq;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Services
{
    /// <summary>
    /// Service for generating pallet numbers
    /// </summary>
    public class PalletNumberGenerator : IPalletNumberGenerator
    {
        private readonly IPalletRepository _palletRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletNumberGenerator"/> class
        /// </summary>
        /// <param name="palletRepository">The pallet repository</param>
        public PalletNumberGenerator(IPalletRepository palletRepository)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
        }

        /// <inheritdoc/>
        public async Task<PalletNumber> GenerateTemporaryNumberAsync(Division division)
        {
            int sequenceNumber = await _palletRepository.GetNextTemporarySequenceNumberAsync();
            return PalletNumber.CreateTemporary(sequenceNumber, division);
        }

        /// <inheritdoc/>
        public async Task<PalletNumber> GeneratePermanentNumberAsync(Division division)
        {
            int sequenceNumber = await _palletRepository.GetNextPermanentSequenceNumberAsync(division);
            return PalletNumber.CreatePermanent(sequenceNumber, division);
        }

        /// <inheritdoc/>
        public async Task<bool> PalletNumberExistsAsync(string palletNumber)
        {
            return await _palletRepository.ExistsAsync(p => p.PalletNumber.Value == palletNumber);
        }

        /// <inheritdoc/>
        public bool ValidatePalletNumberFormat(string palletNumber, Division division)
        {
            return ValidatePalletNumberFormatInternal(palletNumber, division);
        }

        /// <summary>
        /// Internal static method for validating pallet number format
        /// </summary>
        /// <param name="palletNumber">The pallet number to validate</param>
        /// <param name="division">The division</param>
        /// <returns>True if the format is valid, false otherwise</returns>
        private static bool ValidatePalletNumberFormatInternal(string palletNumber, Division division)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
                return false;

            // Check if it's a temporary number
            if (palletNumber.StartsWith("TEMP-", StringComparison.OrdinalIgnoreCase))
                return true;

            // Check division-specific format
            switch (division)
            {
                case Division.MA:
                    // Manufacturing: P8 prefix followed by 5 digits
                    return palletNumber.StartsWith("P8") && palletNumber.Length == 7 &&
                           palletNumber.Substring(2).All(char.IsDigit);

                case Division.TC:
                    // Technical Center: 47 prefix followed by 5 digits
                    return palletNumber.StartsWith("47") && palletNumber.Length == 7 &&
                           palletNumber.Substring(2).All(char.IsDigit);

                default:
                    return false;
            }
        }
    }
}