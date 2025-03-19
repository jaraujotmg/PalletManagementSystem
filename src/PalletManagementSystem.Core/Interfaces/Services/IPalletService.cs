using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for managing pallets
    /// </summary>
    public interface IPalletService
    {
        /// <summary>
        /// Gets a pallet by ID
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <returns>The pallet DTO if found, null otherwise</returns>
        Task<PalletDto> GetPalletByIdAsync(int id);

        /// <summary>
        /// Gets a pallet by pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet DTO if found, null otherwise</returns>
        Task<PalletDto> GetPalletByNumberAsync(string palletNumber);

        /// <summary>
        /// Gets pallets filtered by division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="includeItems">Whether to include items in the results</param>
        /// <returns>Collection of pallets matching the criteria</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division, Platform platform, bool includeItems = false);

        /// <summary>
        /// Gets pallets filtered by status (open or closed)
        /// </summary>
        /// <param name="isClosed">Whether to get closed or open pallets</param>
        /// <param name="includeItems">Whether to include items in the results</param>
        /// <returns>Collection of pallets matching the criteria</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(bool isClosed, bool includeItems = false);

        /// <summary>
        /// Gets pallets filtered by division and status
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="isClosed">Whether to get closed or open pallets</param>
        /// <param name="includeItems">Whether to include items in the results</param>
        /// <returns>Collection of pallets matching the criteria</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(
            Division division, bool isClosed, bool includeItems = false);

        /// <summary>
        /// Creates a new pallet
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="unitOfMeasure">The unit of measure</param>
        /// <param name="username">Username of the creator</param>
        /// <returns>The created pallet DTO</returns>
        Task<PalletDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username);

        /// <summary>
        /// Closes a pallet
        /// </summary>
        /// <param name="palletId">The ID of the pallet to close</param>
        /// <param name="autoPrint">Whether to automatically print the pallet list</param>
        /// <param name="notes">Optional notes about the closing</param>
        /// <returns>The updated pallet DTO</returns>
        Task<PalletDto> ClosePalletAsync(int palletId, bool autoPrint = true, string notes = null);

        /// <summary>
        /// Searches for pallets matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="includeItems">Whether to include items in the results</param>
        /// <returns>Collection of pallets matching the search</returns>
        Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword, bool includeItems = false);

        /// <summary>
        /// Gets a pallet with its items
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>The pallet DTO with items included</returns>
        Task<PalletDto> GetPalletWithItemsAsync(int palletId);

        /// <summary>
        /// Gets a pallet with its items by pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet DTO with items included</returns>
        Task<PalletDto> GetPalletWithItemsByNumberAsync(string palletNumber);

        /// <summary>
        /// Gets all pallets
        /// </summary>
        /// <param name="includeItems">Whether to include items in the results</param>
        /// <returns>Collection of all pallets</returns>
        Task<IEnumerable<PalletDto>> GetAllPalletsAsync(bool includeItems = false);
    }
}