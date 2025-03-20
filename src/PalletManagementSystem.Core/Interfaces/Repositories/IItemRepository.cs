using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for item entities
    /// </summary>
    public interface IItemRepository : IRepository<Item>
    {
        /// <summary>
        /// Gets an item by ID as a list DTO (without pallet)
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item list DTO, or null if not found</returns>
        Task<ItemListDto> GetItemListByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by ID as a detail DTO (with pallet)
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item detail DTO, or null if not found</returns>
        Task<ItemDetailDto> GetItemDetailByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by its item number as a list DTO (without pallet)
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item list DTO, or null if not found</returns>
        Task<ItemListDto> GetItemListByNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item by its item number as a detail DTO (with pallet)
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The item detail DTO, or null if not found</returns>
        Task<ItemDetailDto> GetItemDetailByNumberAsync(string itemNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by pallet ID as list DTOs
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching item list DTOs</returns>
        Task<IReadOnlyList<ItemListDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by client code as list DTOs
        /// </summary>
        /// <param name="clientCode">The client code</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching item list DTOs</returns>
        Task<IReadOnlyList<ItemListDto>> GetItemsByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets items by manufacturing order as list DTOs
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching item list DTOs</returns>
        Task<IReadOnlyList<ItemListDto>> GetItemsByManufacturingOrderAsync(string manufacturingOrder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the next item number
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The next item number</returns>
        Task<string> GetNextItemNumberAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for items matching the keyword as list DTOs
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching item list DTOs</returns>
        Task<IReadOnlyList<ItemListDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of items as list DTOs
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="palletId">Optional pallet ID filter</param>
        /// <param name="clientCode">Optional client code filter</param>
        /// <param name="manufacturingOrder">Optional manufacturing order filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="orderByCreatedDate">Order by created date if true, otherwise by item number</param>
        /// <param name="descending">Whether to order in descending order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of item list DTOs</returns>
        Task<PagedResultDto<ItemListDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool orderByCreatedDate = true,
            bool descending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of items as detail DTOs (with pallets)
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="palletId">Optional pallet ID filter</param>
        /// <param name="clientCode">Optional client code filter</param>
        /// <param name="manufacturingOrder">Optional manufacturing order filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="orderByCreatedDate">Order by created date if true, otherwise by item number</param>
        /// <param name="descending">Whether to order in descending order</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of item detail DTOs</returns>
        Task<PagedResultDto<ItemDetailDto>> GetPagedItemDetailsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool orderByCreatedDate = true,
            bool descending = true,
            CancellationToken cancellationToken = default);
    }
}