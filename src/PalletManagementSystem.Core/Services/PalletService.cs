using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PalletService"/> class
        /// </summary>
        /// <param name="palletRepository">The pallet repository</param>
        /// <param name="printerService">The printer service</param>
        public PalletService(IPalletRepository palletRepository, IPrinterService printerService)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByIdAsync(int id)
        {
            var pallet = await _palletRepository.GetByIdAsync(id);
            return pallet != null ? MapToDto(pallet) : null;
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletByNumberAsync(string palletNumber)
        {
            var pallet = await _palletRepository.GetByPalletNumberAsync(palletNumber);
            return pallet != null ? MapToDto(pallet) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(Division division, Platform platform)
        {
            var pallets = await _palletRepository.GetByDivisionAndPlatformAsync(division, platform);
            return pallets.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(bool isClosed)
        {
            var pallets = await _palletRepository.GetByStatusAsync(isClosed);
            return pallets.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(Division division, bool isClosed)
        {
            var pallets = await _palletRepository.GetByDivisionAndStatusAsync(division, isClosed);
            return pallets.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<PalletDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username)
        {
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

        /// <inheritdoc/>
        public async Task<PalletDto> ClosePalletAsync(int palletId)
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

            // Print pallet list
            await _printerService.PrintPalletListAsync(pallet.Id);

            return MapToDto(pallet);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword)
        {
            var pallets = await _palletRepository.SearchAsync(keyword);
            return pallets.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletWithItemsAsync(int palletId)
        {
            var pallet = await _palletRepository.GetByIdWithItemsAsync(palletId);
            return pallet != null ? MapToDtoWithItems(pallet) : null;
        }

        /// <inheritdoc/>
        public async Task<PalletDto> GetPalletWithItemsByNumberAsync(string palletNumber)
        {
            var pallet = await _palletRepository.GetByPalletNumberWithItemsAsync(palletNumber);
            return pallet != null ? MapToDtoWithItems(pallet) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> GetAllPalletsAsync()
        {
            var pallets = await _palletRepository.GetAllAsync();
            return pallets.Select(MapToDto);
        }

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
    }
}