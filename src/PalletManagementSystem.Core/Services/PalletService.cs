using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

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
                var pallet = await _unitOfWork.PalletRepository.GetByIdAsync(id, cancellationToken);
                return pallet != null ? MapToDto(pallet) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with ID {id}");
                throw;
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
                var pallet = await _unitOfWork.PalletRepository.GetByPalletNumberAsync(palletNumber, cancellationToken);
                return pallet != null ? MapToDto(pallet) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with number {palletNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division, Platform platform, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate platform is valid for division
                bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
                if (!isValid)
                {
                    throw new ArgumentException($"Platform {platform} is not valid for division {division}");
                }

                var pallets = includeItems
                    ? await _unitOfWork.PalletRepository.GetByDivisionAndPlatformWithItemsAsync(division, platform, cancellationToken)
                    : await _unitOfWork.PalletRepository.GetByDivisionAndPlatformAsync(division, platform, cancellationToken);

                // Use explicit conversion to avoid type inference issues
                return includeItems
                    ? pallets.Select(p => MapToDtoWithItems(p))
                    : pallets.Select(p => MapToDto(p));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} and platform {platform}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(bool isClosed, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var pallets = await _unitOfWork.PalletRepository.GetByStatusAsync(isClosed, cancellationToken);

                // Use explicit conversion to avoid type inference issues
                return includeItems
                    ? pallets.Select(p => MapToDtoWithItems(p))
                    : pallets.Select(p => MapToDto(p));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets with isClosed={isClosed}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(
            Division division, bool isClosed, bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var pallets = await _unitOfWork.PalletRepository.GetByDivisionAndStatusAsync(division, isClosed, cancellationToken);

                // Use explicit conversion to avoid type inference issues
                return includeItems
                    ? pallets.Select(p => MapToDtoWithItems(p))
                    : pallets.Select(p => MapToDto(p));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} with isClosed={isClosed}");
                throw;
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

                return MapToDto(createdPallet);
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

                return MapToDtoWithItems(pallet);
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
                var pallets = await _unitOfWork.PalletRepository.SearchAsync(keyword, cancellationToken);

                // Use explicit conversion to avoid type inference issues
                return includeItems
                    ? pallets.Select(p => MapToDtoWithItems(p))
                    : pallets.Select(p => MapToDto(p));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets with keyword '{keyword}'");
                throw;
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
                var pallet = await _unitOfWork.PalletRepository.GetByPalletNumberWithItemsAsync(palletNumber, cancellationToken);
                return pallet != null ? MapToDtoWithItems(pallet) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with items for number {palletNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetAllPalletsAsync(bool includeItems = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (includeItems)
                {
                    var pallets = await _unitOfWork.PalletRepository.GetAllWithItemsAsync(cancellationToken);
                    return pallets.Select(p => MapToDtoWithItems(p));
                }
                else
                {
                    var pallets = await _unitOfWork.PalletRepository.GetAllAsync(cancellationToken);
                    return pallets.Select(p => MapToDto(p));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pallets");
                throw;
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

                // Map to DTOs - use explicit mapping based on includeItems flag
                var palletDtos = includeItems
                    ? pagedResult.Items.Select(p => MapToDtoWithItems(p)).ToList()
                    : pagedResult.Items.Select(p => MapToDto(p)).ToList();

                // Create DTO for paged result
                var pagedResultDto = new PagedResultDto<PalletDto>
                {
                    Items = palletDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                return pagedResultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged pallets");
                throw;
            }
        }

        #region Mapping Methods

        /// <summary>
        /// Maps a Pallet entity to a PalletDto
        /// </summary>
        /// <param name="pallet">The pallet entity</param>
        /// <returns>The pallet DTO</returns>
        private static PalletDto MapToDto(Pallet pallet)
        {
            return new PalletDto
            {
                Id = pallet.Id,
                PalletNumber = pallet.PalletNumber.Value,
                IsTemporary = pallet.PalletNumber.IsTemporary,
                ManufacturingOrder = pallet.ManufacturingOrder,
                Division = pallet.Division.ToString(),
                Platform = pallet.Platform.ToString(),
                UnitOfMeasure = pallet.UnitOfMeasure.ToString(),
                Quantity = pallet.Quantity,
                ItemCount = pallet.ItemCount,
                IsClosed = pallet.IsClosed,
                CreatedDate = pallet.CreatedDate,
                ClosedDate = pallet.ClosedDate,
                CreatedBy = pallet.CreatedBy
            };
        }

        /// <summary>
        /// Maps a Pallet entity to a PalletDto including its items
        /// </summary>
        /// <param name="pallet">The pallet entity</param>
        /// <returns>The pallet DTO with items</returns>
        private static PalletDto MapToDtoWithItems(Pallet pallet)
        {
            var dto = MapToDto(pallet);

            // Add items
            dto.Items = pallet.Items.Select(item => new ItemDto
            {
                Id = item.Id,
                ItemNumber = item.ItemNumber,
                PalletId = item.PalletId,
                ManufacturingOrder = item.ManufacturingOrder,
                ManufacturingOrderLine = item.ManufacturingOrderLine,
                ServiceOrder = item.ServiceOrder,
                ServiceOrderLine = item.ServiceOrderLine,
                FinalOrder = item.FinalOrder,
                FinalOrderLine = item.FinalOrderLine,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                Reference = item.Reference,
                Finish = item.Finish,
                Color = item.Color,
                Quantity = item.Quantity,
                QuantityUnit = item.QuantityUnit,
                Weight = item.Weight,
                WeightUnit = item.WeightUnit,
                Width = item.Width,
                WidthUnit = item.WidthUnit,
                Quality = item.Quality,
                Batch = item.Batch,
                CreatedDate = item.CreatedDate,
                CreatedBy = item.CreatedBy
            }).ToList();

            return dto;
        }

        #endregion
    }
}