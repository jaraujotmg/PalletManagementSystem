using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Pallet operations
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
        /// Gets a pallet by its pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet DTO if found, null otherwise</returns>
        Task<PalletDto> GetPalletByNumberAsync(string palletNumber);

        /// <summary>
        /// Gets pallets by division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(Division division, Platform platform);

        /// <summary>
        /// Gets pallets by status (open or closed)
        /// </summary>
        /// <param name="isClosed">True for closed pallets, false for open pallets</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(bool isClosed);

        /// <summary>
        /// Gets pallets for a division by status (open or closed)
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="isClosed">True for closed pallets, false for open pallets</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(Division division, bool isClosed);

        /// <summary>
        /// Creates a new pallet
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="unitOfMeasure">The unit of measure</param>
        /// <param name="username">The creating user's username</param>
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
        /// <param name="palletId">The pallet ID</param>
        /// <returns>The updated pallet DTO</returns>
        Task<PalletDto> ClosePalletAsync(int palletId);

        /// <summary>
        /// Searches for pallets by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of matching pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword);

        /// <summary>
        /// Gets a pallet with items
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>The pallet DTO with items if found, null otherwise</returns>
        Task<PalletDto> GetPalletWithItemsAsync(int palletId);

        /// <summary>
        /// Gets a pallet by number with items
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet DTO with items if found, null otherwise</returns>
        Task<PalletDto> GetPalletWithItemsByNumberAsync(string palletNumber);

        /// <summary>
        /// Gets all pallets
        /// </summary>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> GetAllPalletsAsync();
    }
}