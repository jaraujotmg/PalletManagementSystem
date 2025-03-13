using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Pallet operations
    /// </summary>
    public interface IPalletRepository : IRepository<Pallet>
    {
        /// <summary>
        /// Gets a pallet by its pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet if found, null otherwise</returns>
        Task<Pallet> GetByPalletNumberAsync(string palletNumber);

        /// <summary>
        /// Gets pallets by division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <returns>A collection of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformAsync(Division division, Platform platform);

        /// <summary>
        /// Gets pallets by manufacturing order
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <returns>A collection of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByManufacturingOrderAsync(string manufacturingOrder);

        /// <summary>
        /// Gets pallets by status (open or closed)
        /// </summary>
        /// <param name="isClosed">The status to filter by</param>
        /// <returns>A collection of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByStatusAsync(bool isClosed);

        /// <summary>
        /// Gets pallets for a division by status (open or closed)
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="isClosed">The status to filter by</param>
        /// <returns>A collection of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByDivisionAndStatusAsync(Division division, bool isClosed);

        /// <summary>
        /// Gets pallets with complete item details
        /// </summary>
        /// <returns>A collection of pallets with their items</returns>
        Task<IReadOnlyList<Pallet>> GetAllWithItemsAsync();

        /// <summary>
        /// Gets a pallet with complete item details
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <returns>The pallet with its items if found, null otherwise</returns>
        Task<Pallet> GetByIdWithItemsAsync(int id);

        /// <summary>
        /// Gets a pallet by its pallet number with complete item details
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <returns>The pallet with its items if found, null otherwise</returns>
        Task<Pallet> GetByPalletNumberWithItemsAsync(string palletNumber);

        /// <summary>
        /// Gets the next sequence number for a temporary pallet
        /// </summary>
        /// <returns>The next sequence number</returns>
        Task<int> GetNextTemporarySequenceNumberAsync();

        /// <summary>
        /// Gets the next sequence number for a permanent pallet by division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>The next sequence number</returns>
        Task<int> GetNextPermanentSequenceNumberAsync(Division division);

        /// <summary>
        /// Search pallets by keyword (pallet number, manufacturing order, etc.)
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> SearchAsync(string keyword);
    }
}