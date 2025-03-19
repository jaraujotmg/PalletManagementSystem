using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for item entities
    /// </summary>
    public interface IItemRepository : IRepository<Item>
    {
        /// <summary>
        /// Gets an item by its item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item, or null if not found</returns>
        Task<Item> GetByItemNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by pallet ID
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> GetByPalletIdAsync(int palletId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by manufacturing order
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> GetByManufacturingOrderAsync(string manufacturingOrder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by client code
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> GetByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by client name (partial match)
        /// </summary>
        /// <param name="clientName">The client name</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> GetByClientNameAsync(string clientName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by reference
        /// </summary>
        /// <param name="reference">The reference</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all items with their pallets
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of all items with pallets</returns>
        Task<IReadOnlyList<Item>> GetAllWithPalletsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by ID with its pallet
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item with pallet, or null if not found</returns>
        Task<Item> GetByIdWithPalletAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by item number with its pallet
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item with pallet, or null if not found</returns>
        Task<Item> GetByItemNumberWithPalletAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for items matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching items</returns>
        Task<IReadOnlyList<Item>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next item number
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The next item number</returns>
        Task<string> GetNextItemNumberAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of items
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="palletId">Optional pallet ID filter</param>
        /// <param name="clientCode">Optional client code filter</param>
        /// <param name="manufacturingOrder">Optional manufacturing order filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="includePallets">Whether to include pallets in the results</param>
        /// <param name="orderByCreatedDate">Order by created date if true, otherwise by item number</param>
        /// <param name="descending">Whether to order in descending order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of items</returns>
        Task<PagedResult<Item>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool includePallets = false,
            bool orderByCreatedDate = false,
            bool descending = false,
            CancellationToken cancellationToken = default);
    }
}