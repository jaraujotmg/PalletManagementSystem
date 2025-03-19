using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Defines operations for managing pallets
    /// </summary>
    public interface IPalletService
    {
        /// <summary>
        /// Gets a pallet by its ID
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The pallet DTO or null if not found</returns>
        Task<PalletDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet by its number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The pallet DTO or null if not found</returns>
        Task<PalletDto> GetByNumberAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pallets for a specific division and platform
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of pallet DTOs</returns>
        Task<IReadOnlyList<PalletDto>> GetByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by their status (open or closed)
        /// </summary>
        /// <param name="isClosed">True to get closed pallets, false for open pallets</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of pallet DTOs</returns>
        Task<IReadOnlyList<PalletDto>> GetByStatusAsync(
            bool isClosed,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets pallets by division and status
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="isClosed">True to get closed pallets, false for open pallets</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of pallet DTOs</returns>
        Task<IReadOnlyList<PalletDto>> GetByDivisionAndStatusAsync(
            Division division,
            bool isClosed,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new pallet
        /// </summary>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="unitOfMeasure">The unit of measure</param>
        /// <param name="username">Username of the creator</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created pallet DTO</returns>
        /// <exception cref="ArgumentException">Thrown when invalid inputs are provided</exception>
        /// <exception cref="DomainException">Thrown when domain rules are violated</exception>
        Task<PalletDto> CreateAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes a pallet, assigning it a permanent number if needed
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated pallet DTO</returns>
        /// <exception cref="EntityNotFoundException">Thrown when the pallet is not found</exception>
        /// <exception cref="PalletClosedException">Thrown when the pallet is already closed</exception>
        Task<PalletDto> CloseAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for pallets matching a keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of matching pallet DTOs</returns>
        Task<IReadOnlyList<PalletDto>> SearchAsync(string keyword, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet with its items
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The pallet DTO with items or null if not found</returns>
        Task<PalletDto> GetWithItemsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a pallet with its items by pallet number
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The pallet DTO with items or null if not found</returns>
        Task<PalletDto> GetWithItemsByNumberAsync(string palletNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all pallets
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of all pallet DTOs</returns>
        Task<IReadOnlyList<PalletDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}