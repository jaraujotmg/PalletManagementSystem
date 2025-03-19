using PalletManagementSystem.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for managing items on pallets
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Gets an item by ID
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>The item DTO if found, null otherwise</returns>
        Task<ItemDto> GetItemByIdAsync(int id);

        /// <summary>
        /// Gets an item by item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item DTO if found, null otherwise</returns>
        Task<ItemDto> GetItemByNumberAsync(string itemNumber);

        /// <summary>
        /// Gets all items on a pallet
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>Collection of items on the pallet</returns>
        Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId);

        /// <summary>
        /// Gets all items for a client
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <returns>Collection of items for the client</returns>
        Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode);

        /// <summary>
        /// Creates a new item on a pallet
        /// </summary>
        /// <param name="itemDto">The item data</param>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="username">Username of the creator</param>
        /// <returns>The created item DTO</returns>
        Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username);

        /// <summary>
        /// Updates an item's editable properties
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="weight">The new weight</param>
        /// <param name="width">The new width</param>
        /// <param name="quality">The new quality</param>
        /// <param name="batch">The new batch</param>
        /// <returns>The updated item DTO</returns>
        Task<ItemDto> UpdateItemAsync(int itemId, decimal weight, decimal width, string quality, string batch);

        /// <summary>
        /// Moves an item to another pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <returns>The updated item DTO</returns>
        Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId);

        /// <summary>
        /// Searches for items matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>Collection of items matching the search</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword);

        /// <summary>
        /// Gets an item with its pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <returns>The item DTO with pallet included</returns>
        Task<ItemDto> GetItemWithPalletAsync(int itemId);

        /// <summary>
        /// Gets an item with its pallet by item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item DTO with pallet included</returns>
        Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber);

        /// <summary>
        /// Gets all items
        /// </summary>
        /// <returns>Collection of all items</returns>
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();

        /// <summary>
        /// Validates if an item can be moved to a target pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <returns>True if the move is valid, otherwise false</returns>
        Task<bool> CanMoveItemToPalletAsync(int itemId, int targetPalletId);
    }
}