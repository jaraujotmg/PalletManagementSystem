using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for item operations
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Gets an item by its ID
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO, or null if not found</returns>
        Task<ItemDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by its number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO, or null if not found</returns>
        Task<ItemDto> GetItemByNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by pallet ID
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by client code
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new item and adds it to a pallet
        /// </summary>
        /// <param name="itemDto">The item data</param>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="username">The username of the creator</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The created item DTO</returns>
        Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an item's editable properties
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="weight">The new weight</param>
        /// <param name="width">The new width</param>
        /// <param name="quality">The new quality</param>
        /// <param name="batch">The new batch</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The updated item DTO</returns>
        Task<ItemDto> UpdateItemAsync(
            int itemId, decimal weight, decimal width, string quality, string batch, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves an item to another pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The moved item DTO</returns>
        Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an item can be moved to another pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>True if the item can be moved, false otherwise</returns>
        Task<bool> CanMoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for items matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of matching item DTOs</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item with its pallet by item ID
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO with pallet, or null if not found</returns>
        Task<ItemDto> GetItemWithPalletAsync(int itemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item with its pallet by item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO with pallet, or null if not found</returns>
        Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all items
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of all item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetAllItemsAsync(CancellationToken cancellationToken = default);

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
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of item DTOs</returns>
        Task<PagedResultDto<ItemDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool includePallets = false,
            CancellationToken cancellationToken = default);
    }
}