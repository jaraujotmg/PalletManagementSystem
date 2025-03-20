using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for pallet operations
    /// </summary>
    public interface IPalletService
    {
        /// <summary>
        /// Gets a pallet by its ID (without items)
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet DTO, or null if not found</returns>
        Task<PalletListDto> GetPalletByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet detail by its ID (with items)
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet detail DTO, or null if not found</returns>
        Task<PalletDetailDto> GetPalletDetailByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet by its number (without items)
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet DTO, or null if not found</returns>
        Task<PalletListDto> GetPalletByNumberAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet detail by its number (with items)
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The pallet detail DTO, or null if not found</returns>
        Task<PalletDetailDto> GetPalletDetailByNumberAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletListDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division, Platform platform, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by status (open or closed)
        /// </summary>
        /// <param name="isClosed">A value indicating whether to get closed pallets</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletListDto>> GetPalletsByStatusAsync(
            bool isClosed, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new pallet
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="unitOfMeasure">The unit of measure</param>
        /// <param name="username">The username of the creator</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The created pallet DTO</returns>
        Task<PalletListDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes a pallet
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="autoPrint">Whether to automatically print the pallet list</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The closed pallet detail DTO</returns>
        Task<PalletDetailDto> ClosePalletAsync(
            int palletId, bool autoPrint = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for pallets matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A collection of matching pallet DTOs</returns>
        Task<IEnumerable<PalletListDto>> SearchPalletsAsync(
            string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of pallets
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="division">Optional division filter</param>
        /// <param name="platform">Optional platform filter</param>
        /// <param name="isClosed">Optional status filter</param>
        /// <param name="keyword">Optional search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of pallet DTOs</returns>
        Task<PagedResultDto<PalletListDto>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            CancellationToken cancellationToken = default);
    }
}