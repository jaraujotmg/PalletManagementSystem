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
        /// Gets an item by its ID (without pallet)
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO, or null if not found</returns>
        Task<ItemListDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item detail by its ID (with pallet)
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item detail DTO, or null if not found</returns>
        Task<ItemDetailDto> GetItemDetailByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by its number (without pallet)
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item DTO, or null if not found</returns>
        Task<ItemListDto> GetItemByNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by pallet ID
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of item DTOs</returns>
        Task<IEnumerable<ItemListDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default);

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
        /// <param name="updateDto">The update data</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The updated item detail DTO</returns>
        Task<ItemDetailDto> UpdateItemAsync(
            int itemId, UpdateItemDto updateDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves an item to another pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The moved item detail DTO</returns>
        Task<ItemDetailDto> MoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default);

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
        Task<IEnumerable<ItemListDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of items
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="palletId">Optional pallet ID filter</param>
        /// <param name="clientCode">Optional client code filter</param>
        /// <param name="manufacturingOrder">Optional manufacturing order filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of item DTOs</returns>
        Task<PagedResultDto<ItemListDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of item details
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="palletId">Optional pallet ID filter</param>
        /// <param name="clientCode">Optional client code filter</param>
        /// <param name="manufacturingOrder">Optional manufacturing order filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of item detail DTOs</returns>
        Task<PagedResultDto<ItemDetailDto>> GetPagedItemDetailsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            CancellationToken cancellationToken = default);
    }
}