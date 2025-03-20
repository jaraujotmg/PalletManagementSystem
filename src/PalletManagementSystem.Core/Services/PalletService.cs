using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using PalletManagementSystem.Infrastructure.Data;

namespace PalletManagementSystem.Core.Services
{
    /// <summary>
    /// Implementation of the pallet service
    /// </summary>
    public class PalletService : IPalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrinterService _printerService;
        private readonly IPlatformValidationService _platformValidationService;
        private readonly ILogger<PalletService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletService"/> class
        /// </summary>
        public PalletService(
            IUnitOfWork unitOfWork,
            IPrinterService printerService,
            IPlatformValidationService platformValidationService,
            ILogger<PalletService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the pallet using projection
                return await DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => p.Id == id)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with ID {id}");

                // Fallback to traditional approach if projection fails
                var pallet = await _unitOfWork.PalletRepository.GetByIdAsync(id, cancellationToken);
                return PalletMapper.ToDto(pallet);
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                // Get the pallet using projection
                return await DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with number {palletNumber}");

                // Fallback to traditional approach if projection fails
                var pallet = await _unitOfWork.PalletRepository.GetByPalletNumberAsync(palletNumber, cancellationToken);
                return PalletMapper.ToDto(pallet);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division, Platform platform, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            // Validate platform is valid for division
            bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
            if (!isValid)
            {
                throw new ArgumentException($"Platform {platform} is not valid for division {division}");
            }

            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => p.Division == division && p.Platform == platform);

                // Apply projection based on whether to include items
                if (includeItems)
                {
                    return await query
                        .ProjectToDtoWithItems()
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    return await query
                        .ProjectToDto()
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} and platform {platform}");

                // Fallback to traditional approach if projection fails
                var pallets = includeItems
                    ? await _unitOfWork.PalletRepository.GetByDivisionAndPlatformWithItemsAsync(division, platform, cancellationToken)
                    : await _unitOfWork.PalletRepository.GetByDivisionAndPlatformAsync(division, platform, cancellationToken);

                return includeItems
                    ? PalletMapper.ToDtoWithItemsList(pallets)
                    : PalletMapper.ToDtoList(pallets);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(
            bool isClosed, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => p.IsClosed == isClosed);

                // Apply projection based on whether to include items
                if (includeItems)
                {
                    return await query
                        .ProjectToDtoWithItems()
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    return await query
                        .ProjectToDto()
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets with isClosed={isClosed}");

                // Fallback to traditional approach if projection fails
                var pallets = await _unitOfWork.PalletRepository.GetByStatusAsync(isClosed, cancellationToken);

                return includeItems
                    ? PalletMapper.ToDtoWithItemsList(pallets)
                    : PalletMapper.ToDtoList(pallets);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(
            Division division, bool isClosed, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => p.Division == division && p.IsClosed == isClosed);

                // Apply projection based on whether to include items
                if (includeItems)
                {
                    return await query
                        .ProjectToDtoWithItems()
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    return await query
                        .ProjectToDto()
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} with isClosed={isClosed}");

                // Fallback to traditional approach if projection fails
                var pallets = await _unitOfWork.PalletRepository.GetByDivisionAndStatusAsync(division, isClosed, cancellationToken);

                return includeItems
                    ? PalletMapper.ToDtoWithItemsList(pallets)
                    : PalletMapper.ToDtoList(pallets);
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username,
            CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Validate platform is valid for division
                bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
                if (!isValid)
                {
                    throw new ArgumentException($"Platform {platform} is not valid for division {division}");
                }

                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Get next temporary sequence number
                int sequenceNumber = await _unitOfWork.PalletRepository.GetNextTemporarySequenceNumberAsync(cancellationToken);

                // Create a temporary pallet number
                var palletNumber = PalletNumber.CreateTemporary(sequenceNumber, division);

                // Create the pallet
                var pallet = new Pallet(
                    palletNumber,
                    manufacturingOrder,
                    division,
                    platform,
                    unitOfMeasure,
                    username);

                // Add to repository
                var createdPallet = await _unitOfWork.PalletRepository.AddAsync(pallet, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return PalletMapper.ToDto(createdPallet);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, $"Error creating pallet for manufacturing order {manufacturingOrder}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> ClosePalletAsync(
            int palletId, bool autoPrint = true, string notes = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var pallet = await _unitOfWork.PalletRepository.GetByIdWithItemsAsync(palletId, cancellationToken);
                if (pallet == null)
                {
                    throw new DomainException($"Pallet with ID {palletId} not found");
                }

                if (pallet.IsClosed)
                {
                    throw new PalletClosedException($"Pallet {pallet.PalletNumber.Value} is already closed");
                }

                // Get permanent pallet number if needed
                PalletNumber permanentNumber = null;
                if (pallet.PalletNumber.IsTemporary)
                {
                    // Get next permanent sequence number for this division
                    int sequenceNumber = await _unitOfWork.PalletRepository.GetNextPermanentSequenceNumberAsync(pallet.Division, cancellationToken);
                    permanentNumber = PalletNumber.CreatePermanent(sequenceNumber, pallet.Division);
                }

                // Close the pallet
                pallet.Close(permanentNumber);

                // Update in repository
                await _unitOfWork.PalletRepository.UpdateAsync(pallet, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Print pallet list if requested
                if (autoPrint)
                {
                    await _printerService.PrintPalletListAsync(pallet.Id);
                }

                return PalletMapper.ToDtoWithItems(pallet);
            }
            catch (PalletClosedException pex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(pex, $"Attempted to close already closed pallet {palletId}");
                throw;
            }
            catch (DomainException dex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(dex, $"Domain error when closing pallet {palletId}");
                throw;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, $"Error closing pallet {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(
            string keyword, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p =>
                        EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword));

                // Apply projection based on whether to include items
                if (includeItems)
                {
                    return await query
                        .ProjectToDtoWithItems()
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    return await query
                        .ProjectToDto()
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets with keyword '{keyword}'");

                // Fallback to traditional approach if projection fails
                var pallets = await _unitOfWork.PalletRepository.SearchAsync(keyword, cancellationToken);

                return includeItems
                    ? PalletMapper.ToDtoWithItemsList(pallets)
                    : PalletMapper.ToDtoList(pallets);
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletWithItemsByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                // Get the pallet with items using projection
                return await DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber)
                    .ProjectToDtoWithItems()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with items for number {palletNumber}");

                // Fallback to traditional approach if projection fails
                var pallet = await _unitOfWork.PalletRepository.GetByPalletNumberWithItemsAsync(palletNumber, cancellationToken);
                return PalletMapper.ToDtoWithItems(pallet);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetAllPalletsAsync(bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork);

                // Apply projection based on whether to include items
                if (includeItems)
                {
                    return await query
                        .ProjectToDtoWithItems()
                        .ToListAsync(cancellationToken);
                }
                else
                {
                    return await query
                        .ProjectToDto()
                        .ToListAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pallets");

                // Fallback to traditional approach if projection fails
                if (includeItems)
                {
                    var pallets = await _unitOfWork.PalletRepository.GetAllWithItemsAsync(cancellationToken);
                    return PalletMapper.ToDtoWithItemsList(pallets);
                }
                else
                {
                    var pallets = await _unitOfWork.PalletRepository.GetAllAsync(cancellationToken);
                    return PalletMapper.ToDtoList(pallets);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<PalletDto>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork);

                // Apply filters
                if (division.HasValue)
                {
                    query = query.Where(p => p.Division == division.Value);
                }

                if (platform.HasValue)
                {
                    query = query.Where(p => p.Platform == platform.Value);
                }

                if (isClosed.HasValue)
                {
                    query = query.Where(p => p.IsClosed == isClosed.Value);
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    // Search in pallet number and manufacturing order
                    query = query.Where(p =>
                        EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword));
                }

                // Apply ordering - newest first
                query = query.OrderByDescending(p => p.CreatedDate);

                // Use projection to get paged results
                return await query.ProjectToPagedResultAsync(pageNumber, pageSize, includeItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged pallets");

                // Fallback to traditional approach
                var pagedResult = await _unitOfWork.PalletRepository.GetPagedPalletsAsync(
                    pageNumber,
                    pageSize,
                    division,
                    platform,
                    isClosed,
                    keyword,
                    orderByCreatedDate: true,
                    descending: true,
                    cancellationToken);

                // Map results
                var palletDtos = includeItems
                    ? pagedResult.Items.Select(PalletMapper.ToDtoWithItems).ToList()
                    : pagedResult.Items.Select(PalletMapper.ToDto).ToList();

                // Create DTO for paged result
                return new PagedResultDto<PalletDto>
                {
                    Items = palletDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };
            }
        }
    }
}