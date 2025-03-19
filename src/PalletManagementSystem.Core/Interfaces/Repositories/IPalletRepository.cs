using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for pallet entities
    /// </summary>
    public interface IPalletRepository : IRepository<Pallet>
    {
        /// <summary>
        /// Gets a pallet by its pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet, or null if not found</returns>
        Task<Pallet> GetByPalletNumberAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformAsync(Division division, Platform platform, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by division and platform with items
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets with items</returns>
        Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformWithItemsAsync(Division division, Platform platform, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by manufacturing order
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByManufacturingOrderAsync(string manufacturingOrder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by status (open or closed)
        /// </summary>
        /// <param name="isClosed">A value indicating whether to get closed pallets</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByStatusAsync(bool isClosed, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by division and status
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="isClosed">A value indicating whether to get closed pallets</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> GetByDivisionAndStatusAsync(Division division, bool isClosed, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pallets with their items
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of all pallets with items</returns>
        Task<IReadOnlyList<Pallet>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet by ID with its items
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet with items, or null if not found</returns>
        Task<Pallet> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet by pallet number with its items
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet with items, or null if not found</returns>
        Task<Pallet> GetByPalletNumberWithItemsAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next temporary sequence number
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The next temporary sequence number</returns>
        Task<int> GetNextTemporarySequenceNumberAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next permanent sequence number for a division
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The next permanent sequence number</returns>
        Task<int> GetNextPermanentSequenceNumberAsync(Division division, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for pallets matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching pallets</returns>
        Task<IReadOnlyList<Pallet>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of pallets
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="platform">Optional platform filter</param>
        /// <param name="isClosed">Optional status filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="orderByCreatedDate">Order by created date if true, otherwise by pallet number</param>
        /// <param name="descending">Whether to order in descending order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of pallets</returns>
        Task<PagedResult<Pallet>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            bool orderByCreatedDate = false,
            bool descending = false,
            CancellationToken cancellationToken = default);
    }
}