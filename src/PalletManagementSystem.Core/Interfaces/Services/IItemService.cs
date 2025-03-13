using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for Item operations
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
        /// Gets an item by its item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item DTO if found, null otherwise</returns>
        Task<ItemDto> GetItemByNumberAsync(string itemNumber);

        /// <summary>
        /// Gets items by pallet ID
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>A collection of item DTOs on the specified pallet</returns>
        Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId);

        /// <summary>
        /// Gets items by client code
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <returns>A collection of matching item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode);

        /// <summary>
        /// Creates a new item
        /// </summary>
        /// <param name="item">The item creation DTO</param>
        /// <param name="palletId">The pallet ID to assign the item to</param>
        /// <param name="username">The creating user's username</param>
        /// <returns>The created item DTO</returns>
        Task<ItemDto> CreateItemAsync(ItemDto item, int palletId, string username);

        /// <summary>
        /// Updates an item's editable properties (weight, width, quality, batch)
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
        /// Searches for items by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of matching item DTOs</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword);

        /// <summary>
        /// Gets an item with its pallet details
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <returns>The item DTO with pallet details if found, null otherwise</returns>
        Task<ItemDto> GetItemWithPalletAsync(int itemId);

        /// <summary>
        /// Gets an item by number with its pallet details
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item DTO with pallet details if found, null otherwise</returns>
        Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber);

        /// <summary>
        /// Gets all items
        /// </summary>
        /// <returns>A collection of item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();
    }
}