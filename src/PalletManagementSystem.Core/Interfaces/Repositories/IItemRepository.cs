using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Item operations
    /// </summary>
    public interface IItemRepository : IRepository<Item>
    {
        /// <summary>
        /// Gets an item by its item number
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item if found, null otherwise</returns>
        Task<Item> GetByItemNumberAsync(string itemNumber);

        /// <summary>
        /// Gets items by pallet ID
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>A collection of items on the specified pallet</returns>
        Task<IReadOnlyList<Item>> GetByPalletIdAsync(int palletId);

        /// <summary>
        /// Gets items by manufacturing order
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <returns>A collection of matching items</returns>
        Task<IReadOnlyList<Item>> GetByManufacturingOrderAsync(string manufacturingOrder);

        /// <summary>
        /// Gets items by client code
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <returns>A collection of matching items</returns>
        Task<IReadOnlyList<Item>> GetByClientCodeAsync(string clientCode);

        /// <summary>
        /// Gets items by client name
        /// </summary>
        /// <param name="clientName">The client name</param>
        /// <returns>A collection of matching items</returns>
        Task<IReadOnlyList<Item>> GetByClientNameAsync(string clientName);

        /// <summary>
        /// Gets items by reference
        /// </summary>
        /// <param name="reference">The reference</param>
        /// <returns>A collection of matching items</returns>
        Task<IReadOnlyList<Item>> GetByReferenceAsync(string reference);

        /// <summary>
        /// Gets items with pallet details
        /// </summary>
        /// <returns>A collection of items with their pallets</returns>
        Task<IReadOnlyList<Item>> GetAllWithPalletsAsync();

        /// <summary>
        /// Gets an item with pallet details
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>The item with its pallet if found, null otherwise</returns>
        Task<Item> GetByIdWithPalletAsync(int id);

        /// <summary>
        /// Gets an item by its item number with pallet details
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <returns>The item with its pallet if found, null otherwise</returns>
        Task<Item> GetByItemNumberWithPalletAsync(string itemNumber);

        /// <summary>
        /// Search items by keyword (item number, reference, client, etc.)
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of matching items</returns>
        Task<IReadOnlyList<Item>> SearchAsync(string keyword);

        /// <summary>
        /// Gets the next available item number
        /// </summary>
        /// <returns>The next available item number</returns>
        Task<string> GetNextItemNumberAsync();
    }
}