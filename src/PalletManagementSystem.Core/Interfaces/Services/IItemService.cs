using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing items
    /// </summary>
    public interface IItemService
    {
        #region Retrieval Operations

        /// <summary>
        /// Gets an item by its ID
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
        /// Gets all items in a pallet
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>Collection of item DTOs for the specified pallet</returns>
        Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId);

        /// <summary>
        /// Gets items by client code
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <returns>Collection of item DTOs for the specified client</returns>
        Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode);

        /// <summary>
        /// Gets an item with its associated pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <returns>The item DTO with pallet information if found, null otherwise</returns>
        Task<ItemDto> GetItemWithPalletAsync(int itemId);

        /// <summary>
        /// Gets an item with its associated pallet by item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item DTO with pallet information if found, null otherwise</returns>
        Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber);

        /// <summary>
        /// Gets all items
        /// </summary>
        /// <returns>Collection of all item DTOs</returns>
        Task<IEnumerable<ItemDto>> GetAllItemsAsync();

        /// <summary>
        /// Searches for items based on a keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>Collection of matching item DTOs</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword);

        #endregion

        #region Modification Operations

        /// <summary>
        /// Creates a new item on a pallet
        /// </summary>
        /// <param name="itemDto">The item information</param>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="username">Username of the creator</param>
        /// <returns>The created item DTO</returns>
        /// <exception cref="EntityNotFoundException">Thrown when the specified pallet doesn't exist</exception>
        /// <exception cref="PalletClosedException">Thrown when attempting to add an item to a closed pallet</exception>
        /// <exception cref="ArgumentNullException">Thrown when itemDto or username is null</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails for item properties</exception>
        Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username);

        /// <summary>
        /// Updates an item's editable properties (weight, width, quality, batch)
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="weight">The new weight</param>
        /// <param name="width">The new width</param>
        /// <param name="quality">The new quality</param>
        /// <param name="batch">The new batch</param>
        /// <returns>The updated item DTO</returns>
        /// <exception cref="EntityNotFoundException">Thrown when the specified item doesn't exist</exception>
        /// <exception cref="PalletClosedException">Thrown when attempting to update an item on a closed pallet</exception>
        /// <exception cref="ItemValidationException">Thrown when validation fails for updated properties</exception>
        Task<ItemDto> UpdateItemAsync(int itemId, decimal weight, decimal width, string quality, string batch);

        /// <summary>
        /// Moves an item to another pallet
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <returns>The updated item DTO</returns>
        /// <exception cref="EntityNotFoundException">Thrown when the specified item or target pallet doesn't exist</exception>
        /// <exception cref="PalletClosedException">Thrown when attempting to move an item from or to a closed pallet</exception>
        Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId);

        #endregion
    }
}