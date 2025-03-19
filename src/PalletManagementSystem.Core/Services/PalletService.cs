using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPalletRepository _palletRepository;
        private readonly IPrinterService _printerService;
        private readonly IPlatformValidationService _platformValidationService;
        private readonly ILogger<PalletService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletService"/> class
        /// </summary>
        public PalletService(
            IPalletRepository palletRepository,
            IPrinterService printerService,
            IPlatformValidationService platformValidationService,
            ILogger<PalletService> logger)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByIdAsync(int id)
        {
            try
            {
                var pallet = await _palletRepository.GetByIdAsync(id);
                return pallet != null ? MapToDto(pallet) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with ID {id}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByNumberAsync(string palletNumber)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                var pallet = await _palletRepository.GetByPalletNumberAsync(palletNumber);
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
            Division division, Platform platform, bool includeItems = false)
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
                    ? await _palletRepository.GetByDivisionAndPlatformWithItemsAsync(division, platform)
                    : await _palletRepository.GetByDivisionAndPlatformAsync(division, platform);

                return pallets.Select(includeItems ? MapToDtoWithItems : MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} and platform {platform}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(bool isClosed, bool includeItems = false)
        {
            try
            {
                var pallets = await _palletRepository.GetByStatusAsync(isClosed);
                return pallets.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets with isClosed={isClosed}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(
            Division division, bool isClosed, bool includeItems = false)
        {
            try
            {
                var pallets = await _palletRepository.GetByDivisionAndStatusAsync(division, isClosed);
                return pallets.Select(MapToDto);
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
            string username)
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

                // Get next temporary sequence number
                int sequenceNumber = await _palletRepository.GetNextTemporarySequenceNumberAsync();

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

                // Save to repository
                var createdPallet = await _palletRepository.AddAsync(pallet);

                return MapToDto(createdPallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating pallet for manufacturing order {manufacturingOrder}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> ClosePalletAsync(int palletId, bool autoPrint = true, string notes = null)
        {
            try
            {
                var pallet = await _palletRepository.GetByIdWithItemsAsync(palletId);
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
                    int sequenceNumber = await _palletRepository.GetNextPermanentSequenceNumberAsync(pallet.Division);
                    permanentNumber = PalletNumber.CreatePermanent(sequenceNumber, pallet.Division);
                }

                // Close the pallet
                pallet.Close(permanentNumber);

                // Update in repository
                await _palletRepository.UpdateAsync(pallet);

                // Print pallet list if requested
                if (autoPrint)
                {
                    await _printerService.PrintPalletListAsync(pallet.Id);
                }

                return MapToDtoWithItems(pallet);
            }
            catch (PalletClosedException pex)
            {
                _logger.LogWarning(pex, $"Attempted to close already closed pallet {palletId}");
                throw;
            }
            catch (DomainException dex)
            {
                _logger.LogWarning(dex, $"Domain error when closing pallet {palletId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing pallet {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword, bool includeItems = false)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                var pallets = await _palletRepository.SearchAsync(keyword);
                return pallets.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets with keyword '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletWithItemsByNumberAsync(string palletNumber)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                var pallet = await _palletRepository.GetByPalletNumberWithItemsAsync(palletNumber);
                return pallet != null ? MapToDtoWithItems(pallet) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with items for number {palletNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetAllPalletsAsync(bool includeItems = false)
        {
            try
            {
                if (includeItems)
                {
                    var pallets = await _palletRepository.GetAllWithItemsAsync();
                    return pallets.Select(MapToDtoWithItems);
                }
                else
                {
                    var pallets = await _palletRepository.GetAllAsync();
                    return pallets.Select(MapToDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pallets");
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